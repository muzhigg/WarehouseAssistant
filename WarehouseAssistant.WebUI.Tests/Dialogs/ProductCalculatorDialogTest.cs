using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Bunit.Rendering;
using FluentAssertions;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

public class ProductCalculatorDialogTest : MudBlazorTestContext
{
    private Mock<IRepository<Product>> _repositoryMock;
    
    // private Mock<IDialogService>       _dialogServiceMock;
    private Mock<ISnackbar>            _snackbarMock;
    private Mock<ILocalStorageService> _localStorageMock;
    
    public ProductCalculatorDialogTest()
    {
        _repositoryMock = new Mock<IRepository<Product>>();
        // _dialogServiceMock = new Mock<IDialogService>();
        _snackbarMock     = new Mock<ISnackbar>();
        _localStorageMock = new Mock<ILocalStorageService>();
        Services.AddMudBlazorDialog();
        Services.AddSingleton(_repositoryMock.Object);
        
        // Services.AddSingleton(_dialogServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddSingleton(_localStorageMock.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
        Services.AddMudBlazorDialog();
    }
    
    [Fact]
    public async Task Should_LoadSettingsFromLocalStorage_OnInitialization()
    {
        // Arrange
        var settings = new ProductCalculatorDialog.CalculatorSettingsData
        {
            DaysCount                      = 7,
            ConsiderCurrentQuantity        = true,
            MinAvgTurnoverForAdditionByBox = 1.5,
            NeedAddToDb                    = true
        };
        
        _localStorageMock
            .Setup(ls =>
                ls.GetItemAsync<ProductCalculatorDialog.CalculatorSettingsData>(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        
        // Act
        var component = RenderComponent<ProductCalculatorDialog>();
        
        // Assert
        component.Instance.DaysCount.Should().Be(settings.DaysCount);
        component.Instance.ConsiderCurrentQuantity.Should().Be(settings.ConsiderCurrentQuantity);
        component.Instance.MinAvgTurnoverForAdditionByBox.Should().Be(settings.MinAvgTurnoverForAdditionByBox);
        component.Instance.NeedAddToDb.Should().Be(settings.NeedAddToDb);
    }
    
    [Fact]
    public async Task Should_SetParametersForOrderCalculator_OnInitialization()
    {
        // Arrange
        var settings = new ProductCalculatorDialog.CalculatorSettingsData
        {
            DaysCount                      = 7,
            ConsiderCurrentQuantity        = true,
            MinAvgTurnoverForAdditionByBox = 1.5,
            NeedAddToDb                    = true
        };
        
        _localStorageMock
            .Setup(ls =>
                ls.GetItemAsync<ProductCalculatorDialog.CalculatorSettingsData>(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        
        // Act
        var component = RenderComponent<ProductCalculatorDialog>();
        
        // Assert
        CalculationOptions calculatorOptions = component.Instance.GetType().GetField("_options",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(component.Instance) as CalculationOptions;
        
        calculatorOptions.DaysCount.Should().Be(settings.DaysCount);
        calculatorOptions.ConsiderCurrentQuantity.Should().Be(settings.ConsiderCurrentQuantity);
        // Other options can be tested similarly...
    }
    
    [Fact]
    public void Should_DisplayErrorAndCloseDialog_When_NoProductsForCalculation()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var dialogService = Services.GetService<IDialogService>() as DialogService;
        
        var cut = RenderComponent<MudDialogProvider>();
        DialogParameters<ProductCalculatorDialog> parameters = new()
        {
            { inputDialog => inputDialog.ProductTableItems, [] },
        };
        
        // Act
        cut.InvokeAsync((async () =>
            await dialogService!.ShowAsync<ProductCalculatorDialog>("ManualOrderInputDialog",
                parameters)));
        
        // Assert
        _snackbarMock.Verify(s => s.Add("Нет товаров для расчёта", It.IsAny<Severity>(),
            It.IsAny<Action<SnackbarOptions>>(),
            It.IsAny<string>()), Times.Once);
        Assert.Throws<ComponentNotFoundException>(() => cut.FindComponent<ProductCalculatorDialog>());
    }
    
    [Fact]
    public async Task Should_UpdateSettings_WhenParametersChanged()
    {
        // Arrange
        var component = RenderComponent<ProductCalculatorDialog>();
        
        // 1. Test DaysCount
        var initialDaysCount = component.Instance.DaysCount;
        var newDaysCount     = initialDaysCount + 5;
        
        await component.InvokeAsync(() => component.Instance.DaysCount = newDaysCount);
        
        component.Instance.DaysCount.Should().Be(newDaysCount);
        _localStorageMock.Verify(
            ls => ls.SetItemAsync(It.IsAny<string>(),
                It.Is<ProductCalculatorDialog.CalculatorSettingsData>(settings => settings.DaysCount == newDaysCount),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // 2. Test ConsiderCurrentQuantity
        var initialConsiderCurrentQuantity = component.Instance.ConsiderCurrentQuantity;
        var newConsiderCurrentQuantity     = !initialConsiderCurrentQuantity;
        
        await component.InvokeAsync(() => component.Instance.ConsiderCurrentQuantity = newConsiderCurrentQuantity);
        
        component.Instance.ConsiderCurrentQuantity.Should().Be(newConsiderCurrentQuantity);
        _localStorageMock.Verify(
            ls => ls.SetItemAsync(It.IsAny<string>(),
                It.Is<ProductCalculatorDialog.CalculatorSettingsData>(settings =>
                    settings.ConsiderCurrentQuantity == newConsiderCurrentQuantity),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        
        // 3. Test MinAvgTurnoverForAdditionByBox
        var initialMinAvgTurnover = component.Instance.MinAvgTurnoverForAdditionByBox;
        var newMinAvgTurnover     = initialMinAvgTurnover + 1.0;
        
        await component.InvokeAsync(() => component.Instance.MinAvgTurnoverForAdditionByBox = newMinAvgTurnover);
        
        component.Instance.MinAvgTurnoverForAdditionByBox.Should().Be(newMinAvgTurnover);
        _localStorageMock.Verify(
            ls => ls.SetItemAsync(It.IsAny<string>(),
                It.Is<ProductCalculatorDialog.CalculatorSettingsData>(settings =>
                    settings.MinAvgTurnoverForAdditionByBox == newMinAvgTurnover), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
        
        // 4. Test NeedAddToDb
        var initialNeedAddToDb = component.Instance.NeedAddToDb;
        var newNeedAddToDb     = !initialNeedAddToDb;
        
        await component.InvokeAsync(() => component.Instance.NeedAddToDb = newNeedAddToDb);
        
        component.Instance.NeedAddToDb.Should().Be(newNeedAddToDb);
        _localStorageMock.Verify(
            ls => ls.SetItemAsync(It.IsAny<string>(),
                It.Is<ProductCalculatorDialog.CalculatorSettingsData>(
                    settings => settings.NeedAddToDb == newNeedAddToDb),
                It.IsAny<CancellationToken>()), Times.Exactly(4));
    }
    
    
    public async Task
        Should_OpenManualOrderInputDialog_When_AverageTurnoverIsZeroAndAvailableQuantityIsGreaterThanZero()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var dialogService = Services.GetService<IDialogService>() as DialogService;
        
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockDialogService = new Mock<IDialogService>();
        
        var productTableItem = new ProductTableItem
        {
            Article           = "12345",
            Name              = "Test Product",
            AverageTurnover   = 0.0,
            AvailableQuantity = 100,
            QuantityToOrder   = 0
        };
        
        var productTableItems = new[] { productTableItem };
        
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        
        // Setup mock for ManualOrderInputDialog
        var manualDialogResult = DialogResult.Ok(true);
        mockDialogService
            .Setup(ds =>
                ds.Show<ManualOrderInputDialog<ProductTableItem>>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
            .Returns(Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(manualDialogResult)));
        
        await cut.InvokeAsync((async () =>
            await dialogService!.ShowAsync<ProductCalculatorDialog>("ProductCalculatorDialog",
                new DialogParameters { { "ProductTableItems", productTableItems } })));
        cut.FindComponent<ProductCalculatorDialog>().Instance.DialogService = mockDialogService.Object;
        
        // Act
        cut.Find("#submit-calculation-button").Click();
        
        // Assert
        mockDialogService.Verify(ds => ds.Show<ManualOrderInputDialog<ProductTableItem>>(
            It.IsAny<string>(), It.Is<DialogParameters>(p => p.Get<object>("Item") == productTableItem)), Times.Once);
        productTableItem.QuantityToOrder.Should().Be(1);
    }
    
    
    public async Task
        ShouldNot_OpenManualOrderInputDialog_When_AverageTurnoverMoreThanZeroAndAvailableQuantityIsGreaterThanZero()
    {
        // Arrange
        Services.AddMudBlazorDialog();
        var dialogService = Services.GetService<IDialogService>() as DialogService;
        
        var cut = RenderComponent<MudDialogProvider>();
        
        var mockDialogService = new Mock<IDialogService>();
        
        var productTableItem = new ProductTableItem
        {
            Article           = "12345",
            Name              = "Test Product",
            AverageTurnover   = 0.1,
            AvailableQuantity = 100,
            QuantityToOrder   = 0
        };
        
        var productTableItems = new[] { productTableItem };
        
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        
        await cut.InvokeAsync((async () =>
            await dialogService!.ShowAsync<ProductCalculatorDialog>("ProductCalculatorDialog",
                new DialogParameters { { "ProductTableItems", productTableItems } })));
        cut.FindComponent<ProductCalculatorDialog>().Instance.DialogService = mockDialogService.Object;
        
        // Act
        cut.Find("#submit-calculation-button").Click();
        
        // Assert
        mockDialogService.Verify(ds => ds.Show<ManualOrderInputDialog<ProductTableItem>>(
            It.IsAny<string>(), It.Is<DialogParameters>(p => p.Get<object>("Item") == productTableItem)), Times.Never);
    }
}