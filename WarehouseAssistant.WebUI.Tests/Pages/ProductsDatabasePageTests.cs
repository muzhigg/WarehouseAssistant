using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using FluentAssertions;
using FluentAssertions.BUnit;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Dialogs;
using WarehouseAssistant.WebUI.Pages;

namespace WarehouseAssistant.WebUI.Tests.Pages;

public sealed class ProductsDatabasePageTests : MudBlazorTestContext
{
    private readonly Mock<IRepository<Product>> _repositoryMock = new();
    private readonly Mock<ISnackbar>            _snackbarMock   = new();
    
    public ProductsDatabasePageTests()
    {
        Services.AddSingleton(_repositoryMock.Object).AddSingleton(_snackbarMock.Object);
        
        Services.AddMudBlazorResizeListener()
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
    public void ProductsDatabasePage_ShouldLoadAndDisplayProductsCorrectly()
    {
        // Arrange
        Mock<IDialogService> dialogServiceMock = new();
        Services.AddSingleton(dialogServiceMock.Object);
        var products = new List<Product>
        {
            new Product
            {
                Article = "123", Name = "Product 1", Barcode = 111111, QuantityPerBox = 10, QuantityPerShelf = 5
            },
            new Product
            {
                Article = "456", Name = "Product 2", Barcode = 222222, QuantityPerBox = 20, QuantityPerShelf = 10
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
    public void ProductsDatabasePage_AddButton_ShouldInvokeProductFormDialog()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Product>());
        
        var mockDialogService = new Mock<IDialogService>();
        Services.AddSingleton(mockDialogService.Object);
        var dialogReferenceMock = new Mock<IDialogReference>();
        var dialogResult        = DialogResult.Ok(true);
        dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
        mockDialogService.Setup(service =>
                service.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
            .ReturnsAsync(dialogReferenceMock.Object);
        
        var page = RenderComponent<ProductsDatabasePage>();
        
        // Act
        var addButton = page.Find("#db-grid-add-product-button");
        addButton.Click();
        
        // Assert
        mockDialogService.Verify(
            service => service.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>()),
            Times.Once);
    }
}