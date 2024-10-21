using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using FluentAssertions;
using FluentAssertions.BUnit;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Dialogs;
using WarehouseAssistant.WebUI.ProductOrder;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

public class ProductCalculatorDialogTest : MudBlazorTestContext
{
    private Mock<ISnackbar>            _snackbarMock            = new();
    private Mock<ILocalStorageService> _productOrderServiceMock = new();
    
    public ProductCalculatorDialogTest()
    {
        Services.AddSingleton(_snackbarMock.Object)
            .AddSingleton(_productOrderServiceMock.Object)
            // .AddMudBlazorDialog()
            .AddMudBlazorSnackbar()
            .AddMudBlazorResizeListener()
            .AddMudBlazorResizeObserver()
            .AddMudBlazorResizeObserverFactory()
            .AddMudBlazorKeyInterceptor()
            .AddMudBlazorJsEvent()
            .AddMudBlazorScrollManager()
            .AddMudBlazorScrollListener()
            .AddMudBlazorJsApi()
            .AddMudBlazorScrollSpy()
            .AddMudPopoverService()
            .AddMudEventManager()
            .AddMudLocalization();
    }
    
    [Fact]
    public void DaysBasedCalculatorDialog_ShouldOpenCorrectly()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        
        DialogParameters<DaysBasedCalculatorDialog> parameters = new();
        parameters.Add(dialog => dialog.ProductTableItems, productTableItems);
        
        IDialogService dialogService = Services.GetService<IDialogService>();
        
        // Act
        dialogProvider.InvokeAsync(async () =>
        {
            await dialogService
                .ShowAsync<DaysBasedCalculatorDialog>(null, parameters);
        });
        
        var dialog = dialogProvider.FindComponent<DaysBasedCalculatorDialog>();
        
        // Assert
        dialog.Should().NotBeNull();
        dialog.Find("#consider-current-quantity").Should().NotBeNull();
        dialog.Find("[id='submit-calculation-button']").Should().NotBeNull();
    }
    
    [Fact]
    public void DaysBasedCalculatorDialog_CalculateQuantity_ShouldInvokeProductFormDialog_WhenDbReferenceIsNull()
    {
        // Arrange
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        Services.AddSingleton(dialogServiceMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        
        var dialog = RenderComponent<DaysBasedCalculatorDialog>(
            builder => builder.Add(p => p.ProductTableItems, productTableItems));
        
        dialog.Instance.NeedAddToDb = true;
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        // dialog.InvokeAsync(() => dialog.Instance.CalculateQuantity(productTableItems[0]));
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ProductFormDialog>(
                It.IsAny<string>(), It.Is<DialogParameters<ProductFormDialog>>(
                    pairs =>
                        pairs.Get<Product>("EditedProduct").Article == productTableItems[0].Article)),
            Times.Once());
    }
    
    [Fact]
    public void
        DaysBasedCalculatorDialog_CalculateQuantity_ShouldInvokeShowManualInputDialog_WhenAverageTurnoverIsZero()
    {
        // Arrange
        var productTableItem = new ProductTableItem
            { Name = "Product 1", Article = "123", AverageTurnover = 0.0, AvailableQuantity = 100 };
        var dialogServiceMock = new Mock<IDialogService>();
        dialogServiceMock.Setup(service =>
            service.ShowAsync<ManualInputDialog<int>>(It.IsAny<string>(),
                It.IsAny<DialogParameters<ManualInputDialog<int>>>())).ReturnsAsync(() =>
        {
            DialogResult dialogResult = DialogResult.Ok(7);
            return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
        });
        Services.AddSingleton(dialogServiceMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        
        var dialog = RenderComponent<DaysBasedCalculatorDialog>(parameters =>
            parameters.Add(p => p.ProductTableItems, new List<ProductTableItem> { productTableItem }));
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        // dialog.InvokeAsync(() => dialog.Instance.CalculateQuantity(productTableItem));
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ManualInputDialog<int>>(
            It.IsAny<string>(), It.IsAny<DialogParameters<ManualInputDialog<int>>>()), Times.Once);
        productTableItem.QuantityToOrder.Should().Be(7);
        productTableItem.QuantityToOrder.Should().Be(7);
    }
    
    [Fact]
    public void IncrementByPercentageCalculatorDialog_ShouldOpenCorrectly()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        
        DialogParameters<DaysBasedCalculatorDialog> parameters = new();
        parameters.Add(dialog => dialog.ProductTableItems, productTableItems);
        
        IDialogService dialogService = Services.GetService<IDialogService>();
        
        // Act
        dialogProvider.InvokeAsync(async () =>
        {
            await dialogService
                .ShowAsync<IncrementByPercentageCalculatorDialog>(null, parameters);
        });
        
        var dialog = dialogProvider.FindComponent<IncrementByPercentageCalculatorDialog>();
        
        // Assert
        dialog.Should().NotBeNull();
        dialog.Find("#percentage-input").Should().NotBeNull();
        dialog.Find("[id='submit-calculation-button']").Should().NotBeNull();
    }
    
    [Fact]
    public void
        IncrementByPercentageCalculatorDialog_CalculateQuantity_ShouldInvokeProductFormDialog_WhenDbReferenceIsNull()
    {
        // Arrange
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        Services.AddSingleton(dialogServiceMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        
        var dialog = RenderComponent<IncrementByPercentageCalculatorDialog>(
            builder => builder.Add(p => p.ProductTableItems, productTableItems));
        
        dialog.Instance.NeedAddToDb = true;
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        // dialog.InvokeAsync(() => dialog.Instance.CalculateQuantity(productTableItems[0]));
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ProductFormDialog>(
                It.IsAny<string>(), It.Is<DialogParameters<ProductFormDialog>>(
                    pairs =>
                        pairs.Get<Product>("EditedProduct").Article == productTableItems[0].Article)),
            Times.Once());
    }
    
    [Fact]
    public void QuantityPerBoxRoundDialog_ShouldOpenCorrectly()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        Services.AddSingleton(new Mock<IRepository<Product>>().Object);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        
        DialogParameters<DaysBasedCalculatorDialog> parameters = new();
        parameters.Add(dialog => dialog.ProductTableItems, productTableItems);
        
        IDialogService dialogService = Services.GetService<IDialogService>();
        
        // Act
        dialogProvider.InvokeAsync(async () =>
        {
            await dialogService
                .ShowAsync<QuantityPerBoxRoundDialog>(null, parameters);
        });
        
        var dialog = dialogProvider.FindComponent<QuantityPerBoxRoundDialog>();
        
        // Assert
        dialog.Should().NotBeNull();
        dialog.Find("[id='submit-calculation-button']").Should().NotBeNull();
    }
    
    [Fact]
    public void QuantityPerBoxRoundDialog_CalculateQuantity_ShouldInvokeProductFormDialog_WhenDbReferenceIsNull()
    {
        // Arrange
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        Services.AddSingleton(dialogServiceMock.Object);
        var repoMock = new Mock<IRepository<Product>>();
        Services.AddSingleton(repoMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        
        var dialog = RenderComponent<QuantityPerBoxRoundDialog>(
            builder => builder.Add(p => p.ProductTableItems, productTableItems));
        
        dialog.Instance.NeedAddToDb = true;
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        // dialog.InvokeAsync(() => dialog.Instance.CalculateQuantity(productTableItems[0]));
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ProductFormDialog>(
                It.IsAny<string>(), It.Is<DialogParameters<ProductFormDialog>>(
                    pairs =>
                        pairs.Get<Product>("EditedProduct").Article == productTableItems[0].Article)),
            Times.Once());
    }
    
    [Fact]
    public void
        QuantityPerBoxRoundDialog_CalculateQuantity_ShouldInvokeShowManualInputDialog_WhenQuantityPerBoxIsNullOrZero()
    {
        // Arrange
        ProductTableItem productTableItem = new ProductTableItem
        {
            Name              = "Product 1", Article = "123", AverageTurnover = 0.0,
            AvailableQuantity = 10000,
            DbReference = new Product
            {
                Name    = "Product 1",
                Article = "123"
            }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        dialogServiceMock.Setup(service =>
            service.ShowAsync<ManualInputDialog<int>>(It.IsAny<string>(),
                It.IsAny<DialogParameters<ManualInputDialog<int>>>())).ReturnsAsync(() =>
        {
            DialogResult dialogResult = DialogResult.Ok(54);
            return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
        });
        Services.AddSingleton(dialogServiceMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        Services.AddSingleton(new Mock<IRepository<Product>>().Object);
        
        var dialog = RenderComponent<QuantityPerBoxRoundDialog>(parameters =>
            parameters.Add(p => p.ProductTableItems, new List<ProductTableItem> { productTableItem }));
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ManualInputDialog<int>>(
            It.IsAny<string>(), It.IsAny<DialogParameters<ManualInputDialog<int>>>()), Times.Once);
        productTableItem.DbReference.QuantityPerBox.Should().Be(54);
        productTableItem.QuantityToOrder.Should().Be(54);
    }
    
    [Fact]
    public void ShelfQuantityAdjustmentCalculatorDialog_ShouldOpenCorrectly()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        Services.AddSingleton(new Mock<IRepository<Product>>().Object);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        
        DialogParameters<DaysBasedCalculatorDialog> parameters = new();
        parameters.Add(dialog => dialog.ProductTableItems, productTableItems);
        
        IDialogService dialogService = Services.GetService<IDialogService>();
        
        // Act
        dialogProvider.InvokeAsync(async () =>
        {
            await dialogService
                .ShowAsync<ShelfQuantityAdjustmentCalculatorDialog>(null, parameters);
        });
        
        var dialog = dialogProvider.FindComponent<ShelfQuantityAdjustmentCalculatorDialog>();
        
        // Assert
        dialog.Should().NotBeNull();
        dialog.Find("#consider-current-quantity").Should().NotBeNull();
        dialog.Find("[id='submit-calculation-button']").Should().NotBeNull();
    }
    
    [Fact]
    public void
        ShelfQuantityAdjustmentCalculatorDialog_CalculateQuantity_ShouldInvokeProductFormDialog_WhenDbReferenceIsNull()
    {
        // Arrange
        var productTableItems = new List<ProductTableItem>
        {
            new ProductTableItem { Name = "Product 1", Article = "123" },
            new ProductTableItem { Name = "Product 2", Article = "456" }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        Services.AddSingleton(dialogServiceMock.Object);
        var repoMock = new Mock<IRepository<Product>>();
        Services.AddSingleton(repoMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        
        var dialog = RenderComponent<ShelfQuantityAdjustmentCalculatorDialog>(
            builder => builder.Add(p => p.ProductTableItems, productTableItems));
        
        dialog.Instance.NeedAddToDb = true;
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ProductFormDialog>(
                It.IsAny<string>(), It.Is<DialogParameters<ProductFormDialog>>(
                    pairs =>
                        pairs.Get<Product>("EditedProduct").Article == productTableItems[0].Article)),
            Times.Once());
    }
    
    [Fact]
    public void
        ShelfQuantityAdjustmentCalculatorDialog_CalculateQuantity_ShouldInvokeShowManualInputDialog_WhenQuantityPerShelfIsNullOrZero()
    {
        // Arrange
        ProductTableItem productTableItem = new ProductTableItem
        {
            Name              = "Product 1", Article = "123", AverageTurnover = 0.0,
            AvailableQuantity = 10000,
            DbReference = new Product
            {
                Name    = "Product 1",
                Article = "123"
            }
        };
        var dialogServiceMock = new Mock<IDialogService>();
        dialogServiceMock.Setup(service =>
            service.ShowAsync<ManualInputDialog<int>>(It.IsAny<string>(),
                It.IsAny<DialogParameters<ManualInputDialog<int>>>())).ReturnsAsync(() =>
        {
            DialogResult dialogResult = DialogResult.Ok(15);
            return Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult));
        });
        Services.AddSingleton(dialogServiceMock.Object);
        ComponentFactories.AddStub<MudDialogInstance>();
        Services.AddSingleton(new Mock<IRepository<Product>>().Object);
        
        var dialog = RenderComponent<ShelfQuantityAdjustmentCalculatorDialog>(parameters =>
            parameters.Add(p => p.ProductTableItems, new List<ProductTableItem> { productTableItem }));
        
        // Act
        dialog.InvokeAsync(() => dialog.Instance.CalculateProducts());
        
        // Assert
        dialogServiceMock.Verify(service => service.ShowAsync<ManualInputDialog<int>>(
            It.IsAny<string>(), It.IsAny<DialogParameters<ManualInputDialog<int>>>()), Times.Once);
        productTableItem.DbReference.QuantityPerShelf.Should().Be(15);
        productTableItem.QuantityToOrder.Should().Be(15);
    }
}