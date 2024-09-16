using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

[TestSubject(typeof(ProductFormDialog))]
public class ProductFormDialogTest : MudBlazorTestContext
{
    private readonly Mock<IDialogService>       _dialogServiceMock;
    private readonly Mock<IRepository<Product>> _repositoryMock;
    private readonly Mock<ISnackbar>            _snackbarMock;
    
    public ProductFormDialogTest()
    {
        _dialogServiceMock = new Mock<IDialogService>();
        _repositoryMock    = new Mock<IRepository<Product>>();
        _snackbarMock      = new Mock<ISnackbar>();
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
    }
    
    [Fact]
    public async Task ShowAddDialogAsync_HappyPath_ReturnsProduct()
    {
        // Arrange
        var productTableItem    = new ProductTableItem { Article = "123", Name = "Test Product" };
        var dialogReferenceMock = new Mock<IDialogReference>();
        var dialogResult        = DialogResult.Ok(new Product { Article = "123", Name = "Test Product" });
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
        _dialogServiceMock.Setup(d =>
                d.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .ReturnsAsync(dialogReferenceMock.Object);
        
        // Act
        var result = await ProductFormDialog.ShowAddDialogAsync(productTableItem, _dialogServiceMock.Object);
        
        // Assert
        result.Should().NotBeNull();
        result.Article.Should().Be("123");
        result.Name.Should().Be("Test Product");
    }
    
    [Fact]
    public async Task ShowAddDialogAsync_DialogCanceled_ReturnsNull()
    {
        // Arrange
        var productTableItem    = new ProductTableItem { Article = "123", Name = "Test Product" };
        var dialogReferenceMock = new Mock<IDialogReference>();
        var dialogResult        = DialogResult.Cancel();
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
        _dialogServiceMock.Setup(d =>
                d.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .ReturnsAsync(dialogReferenceMock.Object);
        
        // Act
        var result = await ProductFormDialog.ShowAddDialogAsync(productTableItem, _dialogServiceMock.Object);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task ShowEditDialogAsync_HappyPath_ReturnsProduct()
    {
        // Arrange
        var product             = new Product { Article = "123", Name = "Test Product" };
        var dialogReferenceMock = new Mock<IDialogReference>();
        var dialogResult        = DialogResult.Ok(product);
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
        _dialogServiceMock.Setup(d =>
                d.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .ReturnsAsync(dialogReferenceMock.Object);
        _dialogServiceMock.Setup(d =>
                d.Show<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .Returns(dialogReferenceMock.Object);
        
        // Act
        var result = await ProductFormDialog.ShowEditDialogAsync(product, _dialogServiceMock.Object);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.Article);
        Assert.Equal("Test Product", result.Name);
    }
    
    [Fact]
    public async Task ShowEditDialogAsync_DialogCanceled_ReturnsNull()
    {
        // Arrange
        var product             = new Product { Article = "123", Name = "Test Product" };
        var dialogReferenceMock = new Mock<IDialogReference>();
        var dialogResult        = DialogResult.Cancel();
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
        _dialogServiceMock.Setup(d =>
                d.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .ReturnsAsync(dialogReferenceMock.Object);
        _dialogServiceMock.Setup(d =>
                d.Show<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
            .Returns(dialogReferenceMock.Object);
        
        // Act
        var result = await ProductFormDialog.ShowEditDialogAsync(product, _dialogServiceMock.Object);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void Should_ThrowException_When_ItemParameterIsMissing()
    {
        // Act
        Services.AddSingleton(_dialogServiceMock.Object);
        
        Action act = () => RenderComponent<ProductFormDialog>(parameters => parameters
            .Add(p => p.IsEditMode, true));
        
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("EditedProduct is null");
    }
    
    [Fact]
    public async Task Submit_AddMode_CallsAddAsync()
    {
        // Arrange
        _repositoryMock.SetupGet(repository => repository.CanWrite).Returns(true);
        
        Services.AddMudBlazorDialog();
        
        var cut = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>() as DialogService;
        DialogParameters<ProductFormDialog> parameters = [];
        parameters.Add(dialog => dialog.IsEditMode, false);
        parameters.Add(dialog => dialog.EditedProduct, new Product { Article = "123", Name = "Test Product" });
        cut.InvokeAsync(() => dialogService.Show<ProductFormDialog>("Add Product",
            parameters));
        await cut.InvokeAsync(async () => await cut.FindComponent<MudForm>().Instance.Validate());
        
        // Act
        cut.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }
    
    [Fact]
    public async Task Submit_EditMode_CallsUpdateAsync()
    {
        // Arrange
        _repositoryMock.SetupGet(repository => repository.CanWrite).Returns(true);
        
        Services.AddMudBlazorDialog();
        
        var cut = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>() as DialogService;
        DialogParameters<ProductFormDialog> parameters = [];
        parameters.Add(dialog => dialog.IsEditMode, true);
        parameters.Add(dialog => dialog.EditedProduct, new Product { Article = "123", Name = "Test Product" });
        cut.InvokeAsync(() => dialogService.Show<ProductFormDialog>("Add Product",
            parameters));
        await cut.InvokeAsync(async () => await cut.FindComponent<MudForm>().Instance.Validate());
        
        // Act
        cut.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }
    
    [Fact]
    public async Task Submit_ThrowsHttpRequestException_ShowsSnackbar()
    {
        // Arrange
        _repositoryMock.SetupGet(repository => repository.CanWrite).Returns(true);
        
        Services.AddMudBlazorDialog();
        
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>())).ThrowsAsync(new HttpRequestException());
        
        var cut = RenderComponent<MudDialogProvider>();
        var dialogService = Services.GetRequiredService<IDialogService>() as DialogService;
        DialogParameters<ProductFormDialog> parameters = [];
        parameters.Add(dialog => dialog.IsEditMode, false);
        parameters.Add(dialog => dialog.EditedProduct, new Product { Article = "123", Name = "Test Product" });
        cut.InvokeAsync(() => dialogService.Show<ProductFormDialog>("Add Product",
            parameters));
        await cut.InvokeAsync(async () => await cut.FindComponent<MudForm>().Instance.Validate());
        
        // Act
        cut.Find("#product-form-dialog-submit-button").Click();
        
        // Assert
        _snackbarMock.Verify(s => s.Add(It.IsAny<string>(), Severity.Error,
            It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public void ArticleValidation_EmptyArticle_ReturnsErrorMessage()
    {
        // Arrange
        ProductFormDialog dialog = new();
        
        // Act
        var result = dialog.ArticleValidation(string.Empty).Result;
        
        // Assert
        Assert.Equal("Артикул обязателен", result);
    }
    
    [Fact]
    public void ArticleValidation_WhitespaceArticle_ReturnsErrorMessage()
    {
        // Arrange
        var productFormDialog = new ProductFormDialog();
        
        // Act
        var result = productFormDialog.ArticleValidation(" 123 ").Result;
        
        // Assert
        Assert.Equal("Артикул не должен содержать пробелы", result);
    }
}