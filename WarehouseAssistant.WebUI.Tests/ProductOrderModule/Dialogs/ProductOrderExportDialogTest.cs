using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bunit.Rendering;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using MiniExcelLibs;
using Moq;
using MudBlazor;
using WarehouseAssistant.ProductOrderModule;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

[TestSubject(typeof(ProductOrderExportDialog))]
public class ProductOrderExportDialogTest : MudBlazorTestContext
{
    private readonly Mock<ISnackbar>  _snackbarMock  = new();
    private readonly Mock<IJSRuntime> _jsRuntimeMock = new();
    
    public ProductOrderExportDialogTest()
    {
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }
    
    [Fact]
    public void Export_Should_ExportFullTable()
    {
        // Arrange
        List<ProductTableItem> items =
        [
            new() { Article = "1", Name = "Product 1" },
            new() { Article = "2", Name = "Product 2" },
            new() { Article = "3", Name = "Product 3" }
        ];
        
        byte[]? xlsArray = null;
        
        _jsRuntimeMock.Setup(x =>
                x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()))
            .Callback((string method, object[] args) => xlsArray = (byte[])args[1]);
        
        DialogParameters<ProductOrderExportDialog> parameters =
            new() { { x => x.Products, items } };
        
        IRenderedComponent<MudDialogProvider> dialogProvider =
            SetupDialog(out IDialogService dialogService);
        dialogProvider.InvokeAsync(() =>
            dialogService.Show<ProductOrderExportDialog>(null, parameters));
        
        // Act
        var mudSwitch = dialogProvider.FindComponents<MudSwitch<bool>>()
            .First(x => x.Markup.Contains("full-export-switch"));
        
        if (mudSwitch.Instance.Value == false)
            mudSwitch.Find("input").Change(true);
        
        dialogProvider.Find("button[type=submit]").Click();
        
        // Assert
        _snackbarMock.Verify(x =>
                x.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(),
                    It.IsAny<string>()),
            Times.Never);
        _jsRuntimeMock.Verify(x =>
            x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()), Times.Once);
        
        xlsArray.Should().NotBeNull();
        using MemoryStream stream = new(xlsArray);
        stream.GetSheetNames().Count.Should().Be(1);
        stream.Query<ProductTableItem>().Count().Should().Be(3);
        
        Assert.Throws<ComponentNotFoundException>(() =>
            dialogProvider.FindComponent<ProductOrderExportDialog>());
    }
    
    [Fact]
    public void Export_Should_UseDividedMethod()
    {
        // Arrange
        List<ProductTableItem> items =
        [
            new()
            {
                Article     = "1", Name = "Product 1", AvailableQuantity = 10000, QuantityToOrder = 1,
                DbReference = new Product() { Article = "1", Name = "Product 1", QuantityPerBox = 1 }
            },
            new() { Article = "2", Name = "Product 2", AvailableQuantity = 1000, QuantityToOrder = 2 },
            new() { Article = "3", Name = "Product 3" }
        ];
        
        byte[]? xlsArray = null;
        
        _jsRuntimeMock.Setup(x =>
                x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()))
            .Callback((string method, object[] args) => xlsArray = (byte[])args[1]);
        
        DialogParameters<ProductOrderExportDialog> parameters =
            new() { { x => x.Products, items } };
        
        IRenderedComponent<MudDialogProvider> dialogProvider =
            SetupDialog(out IDialogService dialogService);
        dialogProvider.InvokeAsync(() =>
            dialogService.Show<ProductOrderExportDialog>(null, parameters));
        
        // Act
        var mudSwitch = dialogProvider.FindComponents<MudSwitch<bool>>()
            .First(x => x.Markup.Contains("full-export-switch"));
        
        if (mudSwitch.Instance.Value)
            mudSwitch.Find("input").Change(false);
        
        var numField = dialogProvider.FindComponents<MudNumericField<int>>()
            .First(x => x.Markup.Contains("order-size-field"));
        numField.Find("input").Change(1);
        
        dialogProvider.Find("button[type=submit]").Click();
        
        // Assert
        _snackbarMock.Verify(x =>
                x.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(),
                    It.IsAny<string>()),
            Times.Never);
        _jsRuntimeMock.Verify(x =>
            x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()), Times.Once);
        
        xlsArray.Should().NotBeNull();
        using MemoryStream stream = new(xlsArray);
        stream.GetSheetNames().Count.Should().Be(2);
        stream.Query<ProductTableItem>("Order 0").Count().Should().Be(1);
        stream.Query<ProductTableItem>("Order 1").Count().Should().Be(1);
        
        Assert.Throws<ComponentNotFoundException>(() =>
            dialogProvider.FindComponent<ProductOrderExportDialog>());
    }
    
    [Fact]
    public void Export_Should_Cancel_When_NoProducts()
    {
        // Arrange
        List<ProductTableItem> items =
        [
            new()
            {
                Article     = "1", Name = "Product 1", AvailableQuantity = 10000, QuantityToOrder = 0,
                DbReference = new Product() { Article = "1", Name = "Product 1", QuantityPerBox = 1 }
            },
            new() { Article = "2", Name = "Product 2", AvailableQuantity = 1000, QuantityToOrder = 0 },
            new() { Article = "3", Name = "Product 3" }
        ];
        
        byte[]? xlsArray = null;
        
        _jsRuntimeMock.Setup(x =>
                x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()))
            .Callback((string method, object[] args) => xlsArray = (byte[])args[1]);
        
        DialogParameters<ProductOrderExportDialog> parameters =
            new() { { x => x.Products, items } };
        
        IRenderedComponent<MudDialogProvider> dialogProvider =
            SetupDialog(out IDialogService dialogService);
        dialogProvider.InvokeAsync(() =>
            dialogService.Show<ProductOrderExportDialog>(null, parameters));
        
        // Act
        var mudSwitch = dialogProvider.FindComponents<MudSwitch<bool>>()
            .First(x => x.Markup.Contains("full-export-switch"));
        
        if (mudSwitch.Instance.Value)
            mudSwitch.Find("input").Change(false);
        
        var numField = dialogProvider.FindComponents<MudNumericField<int>>()
            .First(x => x.Markup.Contains("order-size-field"));
        numField.Find("input").Change(1);
        
        dialogProvider.Find("button[type=submit]").Click();
        
        // Assert
        _snackbarMock.Verify(x =>
                x.Add(It.IsAny<string>(), Severity.Error, It.IsAny<Action<SnackbarOptions>>(),
                    It.IsAny<string>()),
            Times.Once);
        _jsRuntimeMock.Verify(x =>
            x.InvokeAsync<IJSVoidResult>("DownloadExcelFile", It.IsAny<object[]>()), Times.Never);
        
        xlsArray.Should().BeNull();
        
        Assert.Throws<ComponentNotFoundException>(() =>
            dialogProvider.FindComponent<ProductOrderExportDialog>());
    }
}