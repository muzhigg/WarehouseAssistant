using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp.Dom;
using FluentAssertions;
using FluentAssertions.BUnit;
using Microsoft.AspNetCore.Components;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;
using WarehouseAssistant.WebUI.Pages;
using WarehouseAssistant.WebUI.Tests.Stubs;

namespace WarehouseAssistant.WebUI.Tests.Pages;

[Trait("Category", "UI")]
public sealed class ProductsDatabasePageTests : MudBlazorTestContext
{
    [Fact]
    public async Task PageLoadAndInitialInitialization()
    {
        Mock<IRepository<Product>> mock = new();
        Services.AddScoped<IRepository<Product>>(_ => mock.Object);
        Services.AddMudServices();
        
        mock.Setup(m => m.GetAllAsync()).Returns(async () =>
        {
            await Task.Delay(1000);
            return
            [
                new Product() { Article = "40001234", Name = "Product 1" },
                new Product() { Article = "40005678", Name = "Product 2" },
                new Product() { Article = "40009012", Name = "Product 3" }
            ];
        });
        
        // Open page
        IRenderedComponent<ProductsDatabasePage> page = RenderComponent<ProductsDatabasePage>();
        
        // Verify table is visible
        page.Find(".db-products-grid").Should().NotBeNull();
        
        // Verify table is loading
        page.Find(".mud-table-loading").Should().NotBeNull();
        page.Instance.InProgress.Should().BeTrue();
        
        // Wait for data to load
        await WaitForPropertyChangeAsync(() => page.Instance.InProgress == false);
        
        // Verify table is not loading
        Assert.Throws<ElementNotFoundException>(() => page.Find(".mud-table-loading"));
        
        // Verify table has 3 rows
        page.FindAll(".db-products-grid-row").Count.Should().Be(3);
    }
    
    private static async Task WaitForPropertyChangeAsync(Func<bool> condition, int timeoutMilliseconds = 10000, int pollingIntervalMilliseconds = 100)
    {
        var timeoutTask = Task.Delay(timeoutMilliseconds);
        while (!condition())
        {
            if (await Task.WhenAny(Task.Delay(pollingIntervalMilliseconds), timeoutTask) == timeoutTask)
            {
                throw new TimeoutException("The property did not change within the expected time.");
            }
        }
    }
    
    [Fact]
    public async Task HandleLoadingError()
    {
        Mock<ISnackbar> snackbarMock = new();
        Services.AddSingleton(snackbarMock.Object);
        Mock<IRepository<Product>> repositoryMock = new();
        repositoryMock.Setup(repository => repository.GetAllAsync()).Returns(() => throw new HttpRequestException());
        Services.AddScoped<IRepository<Product>>(_ => repositoryMock.Object);
        Services.AddMudBlazorDialog().AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
        
        // Open page
        IRenderedComponent<ProductsDatabasePage> page = RenderComponent<ProductsDatabasePage>();
        
        // Verify error message is displayed
        snackbarMock.Verify(snackbar => snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""), Times.Once);
    }
    
    [Fact]
    public async Task HandleEditAndDeleteProductError()
    {
        Mock<ISnackbar> snackbarMock = new();
        Services.AddSingleton(snackbarMock.Object);
        Mock<IRepository<Product>> repositoryMock = new();
        repositoryMock.Setup(repository => repository.GetAllAsync()).Returns(async () => new List<Product>
        {
            new() { Article = "40001234", Name = "Product 1" }
        });
        repositoryMock.Setup(repository => repository.UpdateAsync(It.IsAny<Product>()))
            .Throws(new HttpRequestException());
        repositoryMock.Setup(repository => repository.DeleteAsync(It.IsAny<string>()))
            .Throws(new HttpRequestException());
        Services.AddScoped<IRepository<Product>>(_ => repositoryMock.Object);
        Services.AddMudBlazorDialog().AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
        IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
        
        // Open page
        IRenderedComponent<ProductsDatabasePage> page = RenderComponent<ProductsDatabasePage>();
        
        page.Instance.GetType().GetMethod("OnProductChanged", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(page.Instance, [new Product() { Article = "40001234", Name = "Product 1-edited" }]);
        
        // Verify error message is displayed
        snackbarMock.Verify(snackbar => snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""), Times.Once);
        
        page.Instance.GetType()
            .GetMethod("DeleteItems", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(page.Instance, [new List<Product>() { new() { Article = "40001234", Name = "Product 1" } }]);
        
        // Verify error message is displayed
        snackbarMock.Verify(snackbar => snackbar.Add(It.IsAny<string>(), Severity.Error, null, ""), Times.Exactly(2));
    }
    
    [Fact]
    public async Task SearchHandle()
    {
        Mock<IRepository<Product>> repositoryMock = new();
        repositoryMock.Setup(repository => repository.GetAllAsync()).Returns(async () =>
        [
            new Product() { Article = "40001234", Name = "Mandelac" },
            new Product() { Article = "40005678", Name = "Sesretinal" },
            new Product() { Article = "40009012", Name = "Daeses" }
        ]);
        Services.AddScoped<IRepository<Product>>(_ => repositoryMock.Object);
        Services.AddMudServices();
        
        // Open page
        IRenderedComponent<ProductsDatabasePage> page = RenderComponent<ProductsDatabasePage>();
        
        // Input search term
        await page.InvokeAsync(async () => await page.FindComponents<MudTextField<string>>()
            .First(component => component.AsElement().ClassList.Contains("search-field")).Instance.SetText("mandelac"));
        
        // Verify table filter is working
        page.FindAll(".db-products-grid-row").Count.Should().Be(1);
    }
    
    [Fact]
    public async Task AddItem_ShouldAddProductToCollection_WhenDialogReturnsValidProduct()
    {
        // Arrange
        var mockRepository    = new Mock<IRepository<Product>>();
        var mockDialogService = new Mock<IDialogService>();
        var mockSnackbar      = new Mock<ISnackbar>();
        
        var expectedProduct = new Product
        {
            Article          = "12345678",
            Name             = "Test Product",
            Barcode          = 000001,
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
        
        var dialogResult = DialogResult.Ok(expectedProduct);
        
        mockDialogService
            .Setup(ds => ds.ShowAsync<AddDbProductDialog>(It.IsAny<string>()))
            .ReturnsAsync(Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(dialogResult)));
        
        Services.AddSingleton(mockRepository.Object);
        Services.AddSingleton(mockDialogService.Object);
        Services.AddSingleton(mockSnackbar.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
        
        var component = RenderComponent<ProductsDatabasePage>();
        
        // Act
        component.Instance.GetType().GetMethod("ShowAddProductDialog", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(component.Instance, null);
        
        // Assert
        mockDialogService.Verify(ds => ds.ShowAsync<AddDbProductDialog>(It.IsAny<string>()), Times.Once);
        IRenderedComponent<MudDataGrid<Product>> grid = component.FindComponent<MudDataGrid<Product>>();
        grid.Instance.Items.Should().HaveCount(1).And.Contain(expectedProduct);
        
        component.Instance.InProgress.Should().BeFalse();
    }
    
    [Fact]
    public void OnProductChanged_ShouldUpdateProduct_WhenCalled()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<Product>>();
        var mockSnackbar   = new Mock<ISnackbar>();
        
        var existingProduct = new Product
        {
            Article          = "12345678",
            Name             = "Existing Product",
            Barcode          = 000001,
            QuantityPerBox   = 10,
            QuantityPerShelf = 5
        };
        
        mockRepository.Setup(repository => repository.GetAllAsync()).Returns(
            async () => new List<Product> { existingProduct });
        
        // Предположим, что продукт уже загружен в коллекцию
        Services.AddSingleton(mockRepository.Object);
        Services.AddSingleton(mockSnackbar.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService().AddMudBlazorDialog();
        
        var component = RenderComponent<ProductsDatabasePage>();
        
        var updatedProduct = new Product
        {
            Article          = "12345678",        // Article остается тем же, так как это ключ
            Name             = "Updated Product", // Измененное имя
            Barcode          = 000002,          // Измененный штрихкод
            QuantityPerBox   = 20,
            QuantityPerShelf = 10
        };
        
        // Act
        component.Instance.GetType().GetMethod("OnProductChanged", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(component.Instance, [updatedProduct]);
        
        // Assert
        mockRepository.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
            p.Article == updatedProduct.Article &&
            p.Name == updatedProduct.Name &&
            p.Barcode == updatedProduct.Barcode &&
            p.QuantityPerBox == updatedProduct.QuantityPerBox &&
            p.QuantityPerShelf == updatedProduct.QuantityPerShelf
        )), Times.Once);
        
        // Проверяем, что статус загрузки был сброшен после выполнения операции
        component.Instance.InProgress.Should().BeFalse();
    }
}