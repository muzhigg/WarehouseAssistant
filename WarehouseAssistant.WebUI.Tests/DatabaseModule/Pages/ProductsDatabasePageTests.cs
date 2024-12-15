using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using FluentAssertions;
using FluentAssertions.BUnit;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.DatabaseModule.Pages;

namespace WarehouseAssistant.WebUI.Tests.Pages;

public sealed class ProductsDatabasePageTests : MudBlazorTestContext
{
    private readonly Mock<IRepository<Product>>          _repositoryMock               = new();
    private readonly Mock<ISnackbar>                     _snackbarMock                 = new();
    private readonly Mock<IProductFormDialogService>     _productFormDialogServiceMock = new();
    private readonly Mock<IDialogService>                _dialogServiceMock            = new();
    private readonly Mock<ILogger<ProductsDatabasePage>> _loggerMock                   = new();
    
    public ProductsDatabasePageTests()
    {
        Services.AddSingleton(_repositoryMock.Object).AddSingleton(_snackbarMock.Object)
            .AddSingleton(_productFormDialogServiceMock.Object).AddSingleton(_loggerMock.Object)
            .AddSingleton(_dialogServiceMock.Object);
    }
    
    [Fact]
    public async Task RefreshProductsAsync_Should_SetProductsToDataGrid()
    {
        // Arrange
        SetupRepositoryReturnProduct();
        
        // Act
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Assert
        var dataGrid = page.FindComponent<MudDataGrid<Product>>();
        dataGrid.Instance.Items.Should().HaveCount(1);
        dataGrid.Instance.Items.First().Should().BeEquivalentTo(GetProduct());
    }
    
    private void SetupRepositoryReturnProduct()
    {
        _repositoryMock.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<List<Product>?>([
                GetProduct()
            ]));
    }
    
    [Fact]
    public void RefreshProductsAsync_Should_Handle_HttpRequestException()
    {
        // Arrange
        _repositoryMock.Setup(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Throws(() => new HttpRequestException(HttpRequestError.ConnectionError,
                "Error", null, HttpStatusCode.BadRequest));
        
        // Act
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Assert
        page.FindAll(".db-products-grid-row").Should().BeEmpty();
        page.Find("#db-grid-refresh-overlay").Should().NotBeNull();
        _snackbarMock.Verify(snackbar =>
            snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""));
    }
    
    [Fact]
    public void RefreshProductsAsync_Should_Handle_Exception()
    {
        // Arrange
        _repositoryMock.Setup(
                r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        // Act
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Assert
        page.FindAll(".db-products-grid-row").Should().BeEmpty();
        page.Find("#db-grid-refresh-overlay").Should().NotBeNull();
        _snackbarMock.Verify(snackbar =>
            snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""));
    }
    
    [Fact]
    public void ShowAddProductDialogAsync_Should_Add_New_Product()
    {
        // Arrange
        SetupDesktopWindowSize();
        _repositoryMock.Setup(repository =>
                repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<List<Product>?>(null));
        _productFormDialogServiceMock.Setup(service => service.ShowAddDialogAsync())
            .ReturnsAsync(GetProduct());
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.Find("#db-grid-add-product-button").Click();
        
        // Assert
        page.FindAll(".db-products-grid-row").Should().HaveCount(1);
    }
    
    [Fact]
    public void ShowEditProductDialog_Should_ChangeProductInDataGrid()
    {
        // Arrange
        Product product = GetProduct();
        _repositoryMock.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult<List<Product>?>([
                product
            ]));
        _productFormDialogServiceMock.Setup(s =>
                s.ShowEditDialogAsync(product))
            .Callback(() => product.Name = "New Name").Returns(Task.FromResult(true));
        
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.Find(".edit-product-button").Click();
        
        // Assert
        page.Find("td[data-label=Название]").TextContent.Should().Be("New Name");
    }
    
    [Fact]
    public void DeleteItems_Should_Cancel_When_ShowConfirmationDialog_Returns_False()
    {
        // Arrange
        SetupRepositoryReturnProduct();
        _dialogServiceMock.Setup(s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(() => false);
        
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.Find(".delete-product-button").Click();
        
        // Assert
        _repositoryMock.Verify(r =>
            r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public void DeleteItem_Should_DeleteItems()
    {
        // Arrange
        SetupRepositoryReturnProduct();
        _dialogServiceMock.Setup(s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(() => true);
        
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.Find(".delete-product-button").Click();
        
        // Assert
        _repositoryMock.Verify(r =>
            r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        page.FindComponent<DataGrid<Product>>().Instance.Items.Should().HaveCount(0);
    }
    
    [Fact]
    public void DeleteItem_Should_HandleExceptions()
    {
        // Arrange
        SetupRepositoryReturnProduct();
        _dialogServiceMock.Setup(s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(() => true);
        _repositoryMock.Setup(r =>
                r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.Find(".delete-product-button").Click();
        
        // Assert
        _repositoryMock.Verify(r =>
            r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        page.FindComponent<DataGrid<Product>>().Instance.Items.Should().HaveCount(1);
        _snackbarMock.Verify(snackbar =>
            snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""));
    }
    
    private static Product GetProduct()
    {
        return new Product
            { Article = "123", Name = "Product 1", Barcode = "111111", QuantityPerBox = 10, QuantityPerShelf = 5 };
    }
    
    // [Fact]
    public void ProductsDatabasePage_ShouldLoadAndDisplayProductsCorrectly()
    {
        // Arrange
        Mock<IDialogService> dialogServiceMock = new();
        Services.AddSingleton(dialogServiceMock.Object);
        var products = new List<Product>
        {
            new Product
            {
                Article = "123", Name = "Product 1", Barcode = "111111", QuantityPerBox = 10, QuantityPerShelf = 5
            },
            new Product
            {
                Article = "456", Name = "Product 2", Barcode = "222222", QuantityPerBox = 20, QuantityPerShelf = 10
            }
        };
        _repositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);
        
        // Act
        IRenderedComponent<ProductsDatabasePage> page = RenderComponent<ProductsDatabasePage>();
        
        // Assert
        IRenderedComponent<MudDataGrid<Product>> dataGrid = page.FindComponent<MudDataGrid<Product>>();
        dataGrid.Should().NotBeNull();
        
        IRefreshableElementCollection<IElement> rows = dataGrid.FindAll(".db-products-grid-row");
        rows.Should().HaveCount(products.Count);
        
        IHtmlCollection<IElement> firstRowCells = rows.First().QuerySelectorAll(".mud-table-cell");
        firstRowCells[1].TextContent.Should().Be(products[0].Article);
        firstRowCells[2].TextContent.Should().Be(products[0].Name);
        firstRowCells[3].TextContent.Should().Be(products[0].Barcode.ToString());
        firstRowCells[4].TextContent.Should().Be(products[0].QuantityPerBox.ToString());
        firstRowCells[5].TextContent.Should().Be(products[0].QuantityPerShelf.ToString());
    }
    
    
    // [Fact]
    public void ShowAddProductDialog_ShouldCallProductFormDialog_AndRenderNewItem()
    {
        // Arrange
        var newProduct = new Product
        {
            Article = "789", Name = "Product 3", Barcode = "333333", QuantityPerBox = 15, QuantityPerShelf = 7
        };
        Services.AddMudBlazorDialog();
        _productFormDialogServiceMock.Setup(service => service.ShowAddDialogAsync())
            .ReturnsAsync(newProduct);
        
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        // page.InvokeAsync(() => page.Instance.ShowAddProductDialog());
        
        // Assert
        page.FindAll(".db-products-grid-row").Count.Should().Be(1);
        page.FindComponents<DataGrid<Product>>().First().Instance.Items.First().Should().BeEquivalentTo(newProduct);
    }
    
    // [Fact]
    public void ShowAddProductDialog_ShouldCallProductFormDialog_AndHandleNullResult()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        _productFormDialogServiceMock.Setup(service => service.ShowAddDialogAsync())
            .ReturnsAsync((Product?)null);
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        // page.InvokeAsync(() => page.Instance.ShowAddProductDialog());
        
        // Assert
        page.FindAll(".db-products-grid-row").Count.Should().Be(0);
    }
    
    // [Fact]
    // public void DeleteItems_Should_CallRepositoryDeleteRange()
    // {
    //     // Arrange
    //     Services.AddMudBlazorDialog();
    //     var productsToDelete = new List<Product>
    //     {
    //         new Product { Article = "123" },
    //         new Product { Article = "456" }
    //     };
    //     _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(productsToDelete);
    //     _repositoryMock.Setup(repo => repo.DeleteRangeAsync(It.IsAny<IEnumerable<string>>()))
    //         .Returns(Task.CompletedTask).Verifiable();
    //     var page = RenderComponent<ProductsDatabasePage>();
    //     
    //     // Act
    //     page.InvokeAsync(() => page.Instance.DeleteItems(productsToDelete));
    //     
    //     // Assert
    //     _repositoryMock.Verify(repo => repo.DeleteRangeAsync(It.Is<IEnumerable<string>>(articles =>
    //         articles.SequenceEqual(productsToDelete.Select(p => p.Article)))), Times.Once);
    // }
}