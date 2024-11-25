using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Supabase.Postgrest.Exceptions;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.DatabaseModule;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

[TestSubject(typeof(ProductFormDialog))]
public class ProductFormDialogTest : MudBlazorTestContext
{
    private readonly Mock<IDialogService>             _dialogServiceMock;
    private readonly Mock<IRepository<Product>>       _repositoryMock;
    private readonly Mock<ISnackbar>                  _snackbarMock;
    private readonly Mock<ILogger<ProductFormDialog>> _loggerMock;
    
    public ProductFormDialogTest()
    {
        _dialogServiceMock = new Mock<IDialogService>();
        _repositoryMock    = new Mock<IRepository<Product>>();
        _snackbarMock      = new Mock<ISnackbar>();
        _loggerMock        = new Mock<ILogger<ProductFormDialog>>();
        
        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
    }
    
    [Fact]
    public async Task OnParametersSet_Should_Throw_NullReferenceException_When_EditedProduct_Is_Null()
    {
        // Act
        Services.AddSingleton(_dialogServiceMock.Object);
        
        Action act = () => RenderComponent<ProductFormDialog>(
            parameters => parameters
                .Add(p => p.IsEditMode, true));
        
        // Assert
        act.Should().Throw<NullReferenceException>()
            .WithMessage("EditedProduct is null");
    }
    
    private IRenderedComponent<ProductFormDialog> RenderDialog(DialogParameters<ProductFormDialog> parameters)
    {
        Services.AddMudBlazorDialog();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var dialogService  = Services.GetRequiredService<IDialogService>() as DialogService;
        
        dialogProvider.InvokeAsync(() =>
            dialogService.Show<ProductFormDialog>(null, parameters));
        
        return dialogProvider.FindComponent<ProductFormDialog>();
    }
    
    private void VerifyProductUpdate(Product editedProduct)
    {
        _repositoryMock.Verify(repository =>
            repository.UpdateAsync(It.Is<Product>(p =>
                    p.Article == editedProduct.Article &&
                    p.Name == editedProduct.Name &&
                    p.Barcode == editedProduct.Barcode &&
                    p.QuantityPerBox == editedProduct.QuantityPerBox &&
                    p.QuantityPerShelf == editedProduct.QuantityPerShelf),
                It.IsAny<CancellationToken>()), Times.Once);
        
        _snackbarMock.Verify(snackbar =>
                snackbar.Add(It.Is<string>(s => s.Contains("обновлен")),
                    Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()),
            Times.Once);
    }
    
    private void VerifyProductAddition(Product editedProduct)
    {
        _repositoryMock.Verify(repository =>
            repository.AddAsync(It.Is<Product>(p =>
                    p.Article == editedProduct.Article &&
                    p.Name == editedProduct.Name &&
                    p.Barcode == editedProduct.Barcode &&
                    p.QuantityPerBox == editedProduct.QuantityPerBox &&
                    p.QuantityPerShelf == editedProduct.QuantityPerShelf),
                It.IsAny<CancellationToken>()), Times.Once);
        
        _snackbarMock.Verify(snackbar =>
                snackbar.Add(It.Is<string>(s => s.Contains("добавлен")),
                    Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()),
            Times.Once);
    }
    
    private void SetupExceptionHandling(Exception exception)
    {
        _repositoryMock.Setup(repository => repository.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Throws(exception);
    }
    
    private Product CreateTestProduct()
    {
        return new Product
        {
            Article          = "123",
            Name             = "Test Product",
            Barcode          = "1234",
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
    }
    
    [Fact]
    public async Task OnParametersSet_Should_SetValues_From_EditedProduct()
    {
        // Arrange
        Product editedProduct = new()
        {
            Article          = "123",
            Name             = "Test Product",
            Barcode          = "1234",
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
        
        // Act
        var dialogRef = SetupDialog(editedProduct, false);
        // var dialog = RenderDialog(new()
        // {
        //     { dialog => dialog.EditedProduct, editedProduct }
        // });
        
        // Assert
        dialogRef.component.Find("input#article-field")
            .GetAttribute("value").Should().Be(editedProduct.Article);
        dialogRef.component.Find("input#product-name-field")
            .GetAttribute("value").Should().Be(editedProduct.Name);
        dialogRef.component.Find("input#barcode-field")
            .GetAttribute("value").Should().Be(editedProduct.Barcode);
        dialogRef.component.Find("input#quantity-per-box-field")
            .GetAttribute("value")
            .Should().Be(editedProduct.QuantityPerBox.ToString());
        dialogRef.component.Find("input#quantity-per-shelf-field")
            .GetAttribute("value")
            .Should().Be(editedProduct.QuantityPerShelf.ToString());
    }
    
    private (IRenderedComponent<ProductFormDialog> component, Task<IDialogReference> taskRef) SetupDialog(
        Product editedProduct, bool isEditMode)
    {
        Services.AddMudBlazorDialog();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var dialogService  = Services.GetRequiredService<IDialogService>() as DialogService;
        
        var dialogRef = dialogProvider.InvokeAsync(
            async () => await dialogService.ShowAsync<ProductFormDialog>(null, new DialogParameters<ProductFormDialog>
            {
                { d => d.EditedProduct, editedProduct },
                { d => d.IsEditMode, isEditMode }
            }));
        
        return (dialogProvider.FindComponent<ProductFormDialog>(), dialogRef);
    }
    
    [Fact]
    public async Task Submit_Should_Update_Product_UsingRepository()
    {
        // Arrange
        Product editedProduct = new()
        {
            Article          = "123",
            Name             = "Test Product",
            Barcode          = "1234",
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
        
        var dialogRef = SetupDialog(editedProduct, true);
        
        // Act
        dialogRef.component.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        VerifyProductUpdate(editedProduct);
        dialogRef.taskRef.Result.Result.Result.Data.Should().Be(true);
    }
    
    [Fact]
    public async Task Submit_Should_Add_Product_When_Not_Existing()
    {
        // Arrange
        _repositoryMock.Setup(repository =>
                repository.ContainsAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        Product editedProduct = new()
        {
            Article          = "123",
            Name             = "Test Product",
            Barcode          = "1234",
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
        
        var dialogRef = SetupDialog(editedProduct, false);
        
        // Act
        dialogRef.component.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        VerifyProductAddition(editedProduct);
        dialogRef.taskRef.Result.Result.Result.Data.Should().Be(true);
    }
    
    [Fact]
    public async Task Submit_Should_Return_When_Form_Is_Invalid()
    {
        // Arrange
        _repositoryMock.Setup(repository =>
                repository.ContainsAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Services.AddMudBlazorDialog();
        var dialogRef = SetupDialog(new Product(), true);
        
        // Act
        dialogRef.component.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        dialogRef.component.FindComponent<EditForm>().Instance.EditContext.GetValidationMessages()
            .Any().Should().BeTrue();
    }
    
    [Fact]
    public async Task Submit_Should_Return_When_WaitingForDbResponse()
    {
        // Arrange
        _repositoryMock.Setup(repository =>
                repository.ContainsAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r =>
                r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.Delay(TimeSpan.FromSeconds(1)));
        
        var dialogRef = SetupDialog(CreateTestProduct(), false);
        var dialog    = dialogRef.component;
        
        // Act
        dialog.Find("#product-form-dialog-submit-button").Click();
        dialog.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        _repositoryMock.Verify(repository =>
            repository.AddAsync(
                It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    public static TheoryData<Exception> PostgrestExceptions => new TheoryData<Exception>
    {
        new PostgrestException("Test Exception"),
        new Exception("Test Exception")
    };
    
    [Theory]
    [MemberData(nameof(PostgrestExceptions))]
    public async Task Submit_Should_Call_HandleError_When_ThrownException(
        Exception exception)
    {
        // Arrange
        SetupExceptionHandling(exception);
        
        var dialogRef = SetupDialog(CreateTestProduct(), false);
        var dialog    = dialogRef.component;
        
        // Act
        dialog.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        _snackbarMock.Verify(s => s.Add(It.IsAny<string>(), Severity.Error,
            It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
        dialogRef.taskRef.Result.Result.Result.Data.Should().Be(false);
    }
}