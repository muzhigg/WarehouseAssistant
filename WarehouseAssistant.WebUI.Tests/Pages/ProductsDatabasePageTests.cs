using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using FluentAssertions;
using FluentAssertions.BUnit;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.DatabaseModule;

namespace WarehouseAssistant.WebUI.Tests.Pages;

public sealed class ProductsDatabasePageTests : MudBlazorTestContext
{
    private readonly Mock<IRepository<Product>>      _repositoryMock               = new();
    private readonly Mock<ISnackbar>                 _snackbarMock                 = new();
    private readonly Mock<IProductFormDialogService> _productFormDialogServiceMock = new();
    
    public ProductsDatabasePageTests()
    {
        Services.AddSingleton(_repositoryMock.Object).AddSingleton(_snackbarMock.Object)
            .AddSingleton(_productFormDialogServiceMock.Object);
    }
    
    [Fact]
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
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);
        
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
    
    
    [Fact]
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
        page.InvokeAsync(() => page.Instance.ShowAddProductDialog());
        
        // Assert
        page.FindAll(".db-products-grid-row").Count.Should().Be(1);
        page.FindComponents<DataGrid<Product>>().First().Instance.Items.First().Should().BeEquivalentTo(newProduct);
    }
    
    [Fact]
    public void ShowAddProductDialog_ShouldCallProductFormDialog_AndHandleNullResult()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        _productFormDialogServiceMock.Setup(service => service.ShowAddDialogAsync())
            .ReturnsAsync((Product?)null);
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.InvokeAsync(() => page.Instance.ShowAddProductDialog());
        
        // Assert
        page.FindAll(".db-products-grid-row").Count.Should().Be(0);
    }
    
    [Fact]
    public void DeleteItems_Should_CallRepositoryDeleteRange()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var productsToDelete = new List<Product>
        {
            new Product { Article = "123" },
            new Product { Article = "456" }
        };
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(productsToDelete);
        _repositoryMock.Setup(repo => repo.DeleteRangeAsync(It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask).Verifiable();
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        page.InvokeAsync(() => page.Instance.DeleteItems(productsToDelete));
        
        // Assert
        _repositoryMock.Verify(repo => repo.DeleteRangeAsync(It.Is<IEnumerable<string>>(articles =>
            articles.SequenceEqual(productsToDelete.Select(p => p.Article)))), Times.Once);
    }
}