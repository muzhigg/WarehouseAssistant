using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp.Dom;
using FluentAssertions;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;
using WarehouseAssistant.WebUI.Pages;

namespace WarehouseAssistant.WebUI.Tests.Pages;

public sealed class ProductsCalculationPageTests : MudBlazorTestContext
{
    private Mock<IRepository<Product>> _repositoryMock;
    private Mock<IDialogService>       _dialogServiceMock;
    private Mock<ISnackbar>            _snackbarMock;
    
    public ProductsCalculationPageTests()
    {
        _repositoryMock    = new Mock<IRepository<Product>>();
        _dialogServiceMock = new Mock<IDialogService>();
        _snackbarMock      = new Mock<ISnackbar>();
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_dialogServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
    }
    
    [Fact, Trait("Category", "Unit")]
    public void InitializationOfDataOnPageLoad()
    {
        // Arrange
        _repositoryMock.Setup(repository1 => repository1.GetAllAsync()).ReturnsAsync(() => new List<Product>());
        
        // Act
        IRenderedComponent<ProductsCalculationPage> cut = RenderComponent<ProductsCalculationPage>();
        
        // Assert
        _repositoryMock.Verify(repository1 => repository1.GetAllAsync(), Times.Once);
    }
    
    [Fact, Trait("Category", "Unit")]
    public async Task ShowFileUploadDialog_ShouldShowDialogAndUploadProducts()
    {
        // Arrange
        List<ProductTableItem> expectedProducts =
        [
            new ProductTableItem { Article = "123", Name = "Product 1" },
            new ProductTableItem { Article = "456", Name = "Product 2" }
        ];
        
        SetupWorksheetUploadDialog(expectedProducts, true);
        
        IRenderedComponent<ProductsCalculationPage> component = RenderComponent<ProductsCalculationPage>();
        
        // Act
        Task showFileUploadDialogTask =
            component.InvokeAsync(async () => await component.Instance.ShowFileUploadDialogAsync());
        
        // Assert
        component.Instance.InProgress.Should().BeTrue();
        await showFileUploadDialogTask;
        component.Instance.Products.Should().BeEquivalentTo(expectedProducts);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        component.Instance.InProgress.Should().BeFalse();
    }
    
    [Fact, Trait("Category", "Unit")]
    public async Task ShowCalculatorDialog_ShouldOpenDialogAndReloadData_WhenItemsAreSelectedAndResultIsTrue()
    {
        // Arrange
        var selectedItems = new List<ProductTableItem>
        {
            new ProductTableItem { Article = "123", Name = "Product 1" },
            new ProductTableItem { Article = "456", Name = "Product 2" }
        };
        
        var dialogResult = DialogResult.Ok(true);
        
        _dialogServiceMock
            .Setup(ds => ds.ShowAsync<ProductCalculatorDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
            .Returns(async () =>
            {
                await Task.Delay(1000);
                
                return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
            });
        
        ComponentFactories.Add<DataGrid<ProductTableItem>>(
            Mock.Of<DataGrid<ProductTableItem>>(dg => dg.SelectedItems == selectedItems.ToHashSet()));
        
        IRenderedComponent<ProductsCalculationPage> component = RenderComponent<ProductsCalculationPage>();
        
        // Act
        Task showCalculatorDialogTask =
            component.InvokeAsync(async () => await component.Instance.ShowCalculatorDialog());
        
        // Assert
        component.Instance.InProgress.Should().BeTrue();
        await showCalculatorDialogTask;
        _snackbarMock.Verify(snackbar => snackbar.Add("Не выбрано ни одного элемента",
            It.IsAny<Severity>(),
            It.IsAny<Action<SnackbarOptions>>(),
            It.IsAny<string>()), Times.Never);
        _dialogServiceMock.Verify(ds => ds.ShowAsync<ProductCalculatorDialog>(
            "Расчет заказа",
            It.Is<DialogParameters>(p =>
                p.Get<IEnumerable<ProductTableItem>>("ProductTableItems").SequenceEqual(selectedItems))
        ), Times.Once);
        
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Exactly(2));
        component.Instance.InProgress.Should().BeFalse();
    }
    
    [Fact, Trait("Category", "Unit")]
    public async Task ShowCalculatorDialog_ShouldShowError_WhenNoItemsAreSelected()
    {
        // Arrange
        ComponentFactories.Add<DataGrid<ProductTableItem>>(
            Mock.Of<DataGrid<ProductTableItem>>(dg => dg.SelectedItems == new HashSet<ProductTableItem>()));
        
        IRenderedComponent<ProductsCalculationPage> component = RenderComponent<ProductsCalculationPage>();
        
        // Act
        await component.Instance.ShowCalculatorDialog();
        
        // Assert
        _snackbarMock.Verify(sb => sb.Add("Не выбрано ни одного элемента",
                Severity.Error,
                It.IsAny<Action<SnackbarOptions>>(),
                It.IsAny<string>()),
            Times.Once);
        component.Instance.InProgress.Should().BeFalse();
    }
    
    [Fact, Trait("Category", "Unit")]
    public async Task ShowAddToDbDialog_ShouldOpenDialogAndAddProductsToDb()
    {
        // Arrange
        ProductTableItem product        = new ProductTableItem { Article = "4000123", Name = "Product 1" };
        Product          expectedResult = new Product { Article          = "4000123", Name = "Product 1" };
        
        var dialogResult = DialogResult.Ok(expectedResult);
        _dialogServiceMock
            .Setup(ds => ds.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
            .Returns(async () =>
            {
                await Task.Delay(1000);
                return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
            });
        
        var cut = RenderComponent<ProductsCalculationPage>();
        
        // Act
        Task showAddToDbDialogTask = cut.InvokeAsync(async () => await cut.Instance.ShowAddToDbDialog(product));
        
        // Assert
        cut.Instance.InProgress.Should().BeTrue();
        await showAddToDbDialogTask;
        _dialogServiceMock.Verify(ds => ds.ShowAsync<ProductFormDialog>(
            "Добавить товар",
            It.Is<DialogParameters>(p =>
                p.Get<string>("ProductName") == product.Name)
        ), Times.Once);
        _snackbarMock.Verify(snackbar => snackbar.Add("Товар успешно добавлен в базу данных",
            Severity.Success,
            It.IsAny<Action<SnackbarOptions>>(),
            It.IsAny<string>()), Times.Once);
        cut.Instance.InProgress.Should().BeFalse();
        cut.Instance.GetType().GetField("_dbProducts", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(cut.Instance).Should().BeEquivalentTo(new List<Product> { expectedResult });
    }
    
    [Fact, Trait("Category", "UI")]
    public async Task ShouldDisplayProduct_ShouldFilterByCaseInsensitiveName()
    {
        // Arrange
        var products = new List<ProductTableItem>
        {
            new ProductTableItem { Article = "123", Name = "MANDELAC" },
            new ProductTableItem { Article = "456", Name = "DAESES" }
        };
        
        SetupWorksheetUploadDialog(products);
        
        IRenderedComponent<ProductsCalculationPage> cut = RenderComponent<ProductsCalculationPage>();
        
        cut.Find("#open-upload-table-dialog-button").Click();
        
        // Act
        cut.Find("#search-product-field").Change("mandelac");
        
        // Assert
        cut.FindAll(".product-row").Count.Should().Be(1);
    }
    
    private void SetupWorksheetUploadDialog(List<ProductTableItem> products, bool delay = false)
    {
        DialogResult? dialogResult = DialogResult.Ok(products);
        
        _dialogServiceMock
            .Setup(ds =>
                ds.ShowAsync<WorksheetUploadDialog<ProductTableItem>>(
                    It.IsAny<string>(), It.IsAny<DialogParameters>()))
            .Returns(async () =>
            {
                if (delay)
                {
                    await Task.Delay(3000);
                }
                
                return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
            });
    }
    
    [Fact, Trait("Category", "UI")]
    public async Task QuantityToOrderField_ShouldChangeObject_WhenInputMoreThanZero()
    {
        // Arrange
        var products = new List<ProductTableItem>
        {
            new ProductTableItem { Article = "123", Name = "Product 1", AvailableQuantity = 1000, QuantityToOrder = 10 }
        };
        
        SetupWorksheetUploadDialog(products);
        
        IRenderedComponent<ProductsCalculationPage> cut = RenderComponent<ProductsCalculationPage>();
        
        cut.Find("#open-upload-table-dialog-button").Click();
        
        // Act
        IElement  element       = cut.Find(".quantity-to-order-cell");
        IElement? querySelector = element.QuerySelector("#quantity-to-order-input");
        querySelector.Change(15);
        
        // Assert
        cut.FindAll(".product-row").Count.Should().Be(1);
        products[0].QuantityToOrder.Should().Be(15);
        cut.FindComponent<DataGrid<ProductTableItem>>().Instance.Items.First().QuantityToOrder.Should().Be(15);
    }
    
    [Fact, Trait("Category", "Unit")]
    public void RefreshDbProductList_ShouldDisplayError_WhenExceptionOccurs()
    {
        // Arrange
        var exceptionMessage = "Ошибка при получении данных";
        
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new HttpRequestException(exceptionMessage));
        
        // Act
        var component = RenderComponent<ProductsCalculationPage>();
        
        // Assert
        _snackbarMock.Verify(s =>
            s.Add(It.Is<string>(msg =>
                    msg.Contains(exceptionMessage)),
                Severity.Error,
                It.IsAny<Action<SnackbarOptions>>(),
                It.IsAny<string>()), Times.Once);
    }
    
    [Fact, Trait("Category", "UI")]
    public async Task Should_SetInProgress_When_FileUploadDialogIsOpened()
    {
        // Arrange
        SetupWorksheetUploadDialog(new List<ProductTableItem>(), true);
        
        var component         = RenderComponent<ProductsCalculationPage>();
        var initialInProgress = component.Instance.InProgress;
        
        // Act
        Task showFileUploadDialogTask =
            component.InvokeAsync(async () => await component.Instance.ShowFileUploadDialogAsync());
        
        // Assert
        initialInProgress.Should().BeFalse("Initial state of InProgress should be false");
        component.Find(".mud-table-loading").Should()
            .NotBeNull("Table should be in loading state while dialog is open");
        component.Instance.InProgress.Should().BeTrue("InProgress should be true while dialog is open");
        
        await showFileUploadDialogTask;
        Assert.Throws<ElementNotFoundException>(() => component.Find(".mud-table-loading"));
        component.Instance.InProgress.Should()
            .BeFalse("Final state of InProgress should be false after operation is complete");
        _dialogServiceMock.Verify(
            ds => ds.ShowAsync<WorksheetUploadDialog<ProductTableItem>>(It.IsAny<string>(),
                It.IsAny<DialogParameters>()), Times.Once);
    }
    
    [Fact, Trait("Category", "UI")]
    public async Task InProgress_Should_AffectOnUI_WhenTrue()
    {
        // Arrange
        var component = RenderComponent<ProductsCalculationPage>();
        
        // Act
        await component.InvokeAsync(() => component.Instance.GetType()
            .GetProperty("InProgress", BindingFlags.Public | BindingFlags.Instance)
            .SetValue(component.Instance, true));
        
        // Assert
        component.Find(".mud-table-loading").Should()
            .NotBeNull("Table should be in loading state while InProgress is true");
        component.Find("#open-upload-table-dialog-button").HasAttribute("disabled")
            .Should().BeTrue("Button should be disabled while InProgress is true");
        component.Find("#open-calculator-dialog-button").HasAttribute("disabled")
            .Should().BeTrue("Button should be disabled while InProgress is true");
        component.Find("#search-product-field").HasAttribute("disabled")
            .Should().BeTrue("Search field should be disabled while InProgress is true");
    }
    
    [Fact, Trait("Category", "UI")]
    public async Task InProgress_Should_AffectOnUI_WhenFalse()
    {
        // Arrange
        var selectedItems = new List<ProductTableItem>
        {
            new ProductTableItem { Article = "123", Name = "Product 1" },
            new ProductTableItem { Article = "456", Name = "Product 2" }
        };
        
        var component = RenderComponent<ProductsCalculationPage>();
        
        SetupWorksheetUploadDialog(selectedItems);
        
        await component.InvokeAsync((async () =>
        {
            await component.Instance.ShowFileUploadDialogAsync();
            await component.FindComponent<DataGrid<ProductTableItem>>().Instance
                .SetSelectedItemAsync(selectedItems.First());
        }));
        
        // Act
        await component.InvokeAsync(() => component.Instance.GetType()
            .GetProperty("InProgress", BindingFlags.Public | BindingFlags.Instance)
            .SetValue(component.Instance, false));
        
        // Assert
        Assert.Throws<ElementNotFoundException>(() => component.Find(".mud-table-loading"));
        component.Find("#open-upload-table-dialog-button").HasAttribute("disabled")
            .Should().BeFalse("Button should be enabled while InProgress is false");
        component.Find("#open-calculator-dialog-button").HasAttribute("disabled")
            .Should().BeFalse("Button should be enabled while InProgress is false");
        component.Find("#search-product-field").HasAttribute("disabled")
            .Should().BeFalse("Search field should be enabled while InProgress is false");
    }
}