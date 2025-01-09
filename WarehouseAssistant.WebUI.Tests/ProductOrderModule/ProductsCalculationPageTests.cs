using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.ProductOrderModule;

namespace WarehouseAssistant.WebUI.Tests.Pages;

public sealed class ProductsCalculationPageTests : MudBlazorTestContext
{
    private Mock<IRepository<Product>>      _repositoryMock;
    private Mock<IDialogService>            _dialogServiceMock;
    private Mock<ISnackbar>                 _snackbarMock;
    private Mock<IProductFormDialogService> _productFormDialogServiceMock;
    
    public ProductsCalculationPageTests()
    {
        _repositoryMock               = new Mock<IRepository<Product>>();
        _dialogServiceMock            = new Mock<IDialogService>();
        _snackbarMock                 = new Mock<ISnackbar>();
        _productFormDialogServiceMock = new Mock<IProductFormDialogService>();
        Services.AddSingleton(_productFormDialogServiceMock.Object);
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_dialogServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
    }
    
    [Fact]
    public void QuantityToOrderCellStyleFunc_Should_Not_SetCellAndRowStyle()
    {
        // Arrange
        IRenderedComponent<ProductsCalculationPage> page =
            RenderedPageWithImportButtonStub(out EventCallback<List<ProductTableItem>> callback);
        
        List<ProductTableItem> productTableItems = GetProductTableItems();
        
        // Act
        page.InvokeAsync(() => callback.InvokeAsync(productTableItems));
        
        // Assert
        page.FindAll(".mud-info.mud-info-text").Count.Should().Be(0);
        page.FindAll(".mud-error.mud-error-text").Count.Should().Be(0);
        page.FindAll(".mud-warning.mud-warning-text").Count.Should().Be(0);
    }
    
    private static List<ProductTableItem> GetProductTableItems()
    {
        List<ProductTableItem> productTableItems =
        [
            new ProductTableItem()
            {
                Article           = "1",
                AvailableQuantity = 10000,
                Name              = "Product 1"
            },
            new ProductTableItem()
            {
                Article           = "2",
                AvailableQuantity = 10000,
                Name              = "Product 2"
            }
        ];
        return productTableItems;
    }
    
    private IRenderedComponent<ProductsCalculationPage> RenderedPageWithImportButtonStub(
        out EventCallback<List<ProductTableItem>> callback)
    {
        ComponentFactories.AddStub<TableImportButton<ProductTableItem>>();
        var page = RenderComponent<ProductsCalculationPage>();
        callback = page.FindComponent<Stub<TableImportButton<ProductTableItem>>>().Instance.Parameters["OnParsed"]
            .As<EventCallback<List<ProductTableItem>>>();
        return page;
    }
    
    [Fact]
    public void
        QuantityToOrderCellStyleFunc_Should_SetErrorAndWarningStyle_When_QuantityToOrderGreaterThanMaxCanBeOrdered()
    {
        // Arrange
        var page = RenderedPageWithImportButtonStub(out EventCallback<List<ProductTableItem>> callback);
        List<ProductTableItem> productTableItems = GetProductTableItems();
        page.InvokeAsync(() => callback.InvokeAsync(productTableItems));
        
        // Act
        productTableItems[0].SetQuantityToOrderUnsafe(productTableItems[0].MaxCanBeOrdered + 1);
        productTableItems[1].SetQuantityToOrderUnsafe(productTableItems[0].MaxCanBeOrdered + 1);
        productTableItems[1].QuantityToOrder = 0;
        
        // Assert
        page.Render();
        page.FindAll(".mud-info.mud-info-text").Count.Should().Be(0);
        page.FindAll(".mud-warning.mud-warning-text").Count.Should().Be(1);
        page.FindAll(".mud-error").Count.Should().Be(1);
    }
    
    [Fact]
    public void QuantityToOrderCellStyleFunc_Should_SetInfoStyle_When_QuantityToOrderGreaterThanZero()
    {
        // Arrange
        var page = RenderedPageWithImportButtonStub(out EventCallback<List<ProductTableItem>> callback);
        List<ProductTableItem> productTableItems = GetProductTableItems();
        page.InvokeAsync(() => callback.InvokeAsync(productTableItems));
        
        // Act
        productTableItems[0].QuantityToOrder = 1;
        productTableItems[1].QuantityToOrder = 1;
        productTableItems[1].QuantityToOrder = 0;
        
        // Assert
        page.Render();
        page.FindAll(".mud-info.mud-info-text").Count.Should().Be(1);
        page.FindAll(".mud-warning.mud-warning-text").Count.Should().Be(0);
        page.FindAll(".mud-error.mud-error-text").Count.Should().Be(0);
    }
    
    [Fact]
    public void
        QuantityToOrderCellStyleFunc_Should_SetWarningStyle_When_QuantityToOrderGreaterOrEqualThanMaxCanBeOrdered()
    {
        // Arrange
        var page = RenderedPageWithImportButtonStub(out EventCallback<List<ProductTableItem>> callback);
        List<ProductTableItem> productTableItems = GetProductTableItems();
        page.InvokeAsync(() => callback.InvokeAsync(productTableItems));
        
        // Act
        productTableItems[0].QuantityToOrder = productTableItems[0].MaxCanBeOrdered;
        productTableItems[1].SetQuantityToOrderUnsafe(productTableItems[0].MaxCanBeOrdered + 1);
        
        // Assert
        page.Render();
        page.FindAll(".mud-warning.mud-warning-text").Count.Should().Be(2);
    }
    
    [Fact]
    public void QuantityToOrderCell_Should_SetValue_GreaterThanMaxCanBeOrdered()
    {
        // Arrange
        var page = RenderedPageWithImportButtonStub(out EventCallback<List<ProductTableItem>> callback);
        List<ProductTableItem> productTableItems = GetProductTableItems();
        productTableItems.Remove(productTableItems[1]);
        productTableItems[0].AvailableQuantity = 1;
        page.InvokeAsync(() => callback.InvokeAsync(productTableItems));
        
        // Act
        page.Find(".quantity-to-order-input input").Input(1);
        
        // Assert
        productTableItems[0].QuantityToOrder = 1;
    }
}