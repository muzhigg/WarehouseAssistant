// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Blazored.LocalStorage;
// using Bunit.Rendering;
// using FluentAssertions;
// using Moq;
// using MudBlazor;
// using MudBlazor.Services;
// using WarehouseAssistant.Core.Calculation;
// using WarehouseAssistant.Data.Repositories;
// using WarehouseAssistant.Shared.Models;
// using WarehouseAssistant.Shared.Models.Db;
// using WarehouseAssistant.WebUI.Dialogs;
// using WarehouseAssistant.WebUI.ProductOrder;
//
// namespace WarehouseAssistant.WebUI.Tests.Dialogs;
//
// public class ProductCalculatorDialogTest : MudBlazorTestContext
// {
//     private Mock<IRepository<Product>> _repositoryMock;
//     
//     // private Mock<IDialogService>       _dialogServiceMock;
//     private Mock<ISnackbar>            _snackbarMock;
//     private Mock<ILocalStorageService> _localStorageMock;
//     
//     public ProductCalculatorDialogTest()
//     {
//         _repositoryMock = new Mock<IRepository<Product>>();
//         // _dialogServiceMock = new Mock<IDialogService>();
//         _snackbarMock     = new Mock<ISnackbar>();
//         _localStorageMock = new Mock<ILocalStorageService>();
//         Services.AddMudBlazorDialog();
//         Services.AddSingleton(_repositoryMock.Object);
//         
//         // Services.AddSingleton(_dialogServiceMock.Object);
//         Services.AddSingleton(_snackbarMock.Object);
//         Services.AddSingleton(_localStorageMock.Object);
//         Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
//             .AddMudBlazorScrollManager().AddMudPopoverService();
//         Services.AddMudBlazorDialog();
//     }
//     
//     [Fact]
//     public async Task Should_LoadSettingsFromLocalStorage_OnInitialization()
//     {
//         // Arrange
//         var settings = new BaseProductCalculatorDialog<,>.CalculatorSettingsData
//         {
//             DaysCount                      = 7,
//             ConsiderCurrentQuantity        = true,
//             MinAvgTurnoverForAdditionByBox = 1.5,
//             NeedAddToDb                    = true
//         };
//         
//         _localStorageMock
//             .Setup(ls =>
//                 ls.GetItemAsync<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(It.IsAny<string>(),
//                     It.IsAny<CancellationToken>()))
//             .ReturnsAsync(settings);
//         
//         // Act
//         var component = RenderComponent<BaseProductCalculatorDialog<,>>();
//         
//         // Assert
//         component.Instance.DaysCount.Should().Be(settings.DaysCount);
//         component.Instance.ConsiderCurrentQuantity.Should().Be(settings.ConsiderCurrentQuantity);
//         component.Instance.MinAvgTurnoverForAdditionByBox.Should().Be(settings.MinAvgTurnoverForAdditionByBox);
//         component.Instance.NeedAddToDb.Should().Be(settings.NeedAddToDb);
//     }
//     
//     [Fact]
//     public async Task Should_SetParametersForOrderCalculator_OnInitialization()
//     {
//         // Arrange
//         var settings = new BaseProductCalculatorDialog<,>.CalculatorSettingsData
//         {
//             DaysCount                      = 7,
//             ConsiderCurrentQuantity        = true,
//             MinAvgTurnoverForAdditionByBox = 1.5,
//             NeedAddToDb                    = true
//         };
//         
//         _localStorageMock
//             .Setup(ls =>
//                 ls.GetItemAsync<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(It.IsAny<string>(),
//                     It.IsAny<CancellationToken>()))
//             .ReturnsAsync(settings);
//         
//         // Act
//         var component = RenderComponent<BaseProductCalculatorDialog<,>>();
//         
//         // Assert
//         CalculationOptions calculatorOptions = component.Instance.GetType().GetField("_options",
//                 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//             .GetValue(component.Instance) as CalculationOptions;
//         
//         calculatorOptions.DaysCount.Should().Be(settings.DaysCount);
//         calculatorOptions.ConsiderCurrentQuantity.Should().Be(settings.ConsiderCurrentQuantity);
//         // Other options can be tested similarly...
//     }
//     
//     [Fact]
//     public void Should_DisplayErrorAndCloseDialog_When_NoProductsForCalculation()
//     {
//         // Arrange
//         Services.AddMudBlazorDialog();
//         var dialogService = Services.GetService<IDialogService>() as DialogService;
//         
//         var cut = RenderComponent<MudDialogProvider>();
//         DialogParameters<BaseProductCalculatorDialog<,>> parameters = new()
//         {
//             { inputDialog => inputDialog.ProductTableItems, [] },
//         };
//         
//         // Act
//         cut.InvokeAsync((async () =>
//             await dialogService!.ShowAsync<BaseProductCalculatorDialog<,>>("ManualOrderInputDialog",
//                 parameters)));
//         
//         // Assert
//         _snackbarMock.Verify(s => s.Add("Нет товаров для расчёта", It.IsAny<Severity>(),
//             It.IsAny<Action<SnackbarOptions>>(),
//             It.IsAny<string>()), Times.Once);
//         Assert.Throws<ComponentNotFoundException>(() => cut.FindComponent<BaseProductCalculatorDialog<,>>());
//     }
//     
//     [Fact]
//     public async Task Should_UpdateSettings_WhenParametersChanged()
//     {
//         // Arrange
//         var component = RenderComponent<BaseProductCalculatorDialog<,>>();
//         
//         // 1. Test DaysCount
//         var initialDaysCount = component.Instance.DaysCount;
//         var newDaysCount     = initialDaysCount + 5;
//         
//         await component.InvokeAsync(() => component.Instance.DaysCount = newDaysCount);
//         
//         component.Instance.DaysCount.Should().Be(newDaysCount);
//         _localStorageMock.Verify(
//             ls => ls.SetItemAsync(It.IsAny<string>(),
//                 It.Is<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(settings => settings.DaysCount == newDaysCount),
//                 It.IsAny<CancellationToken>()),
//             Times.Once);
//         
//         // 2. Test ConsiderCurrentQuantity
//         var initialConsiderCurrentQuantity = component.Instance.ConsiderCurrentQuantity;
//         var newConsiderCurrentQuantity     = !initialConsiderCurrentQuantity;
//         
//         await component.InvokeAsync(() => component.Instance.ConsiderCurrentQuantity = newConsiderCurrentQuantity);
//         
//         component.Instance.ConsiderCurrentQuantity.Should().Be(newConsiderCurrentQuantity);
//         _localStorageMock.Verify(
//             ls => ls.SetItemAsync(It.IsAny<string>(),
//                 It.Is<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(settings =>
//                     settings.ConsiderCurrentQuantity == newConsiderCurrentQuantity),
//                 It.IsAny<CancellationToken>()), Times.Exactly(2));
//         
//         // 3. Test MinAvgTurnoverForAdditionByBox
//         var initialMinAvgTurnover = component.Instance.MinAvgTurnoverForAdditionByBox;
//         var newMinAvgTurnover     = initialMinAvgTurnover + 1.0;
//         
//         await component.InvokeAsync(() => component.Instance.MinAvgTurnoverForAdditionByBox = newMinAvgTurnover);
//         
//         component.Instance.MinAvgTurnoverForAdditionByBox.Should().Be(newMinAvgTurnover);
//         _localStorageMock.Verify(
//             ls => ls.SetItemAsync(It.IsAny<string>(),
//                 It.Is<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(settings =>
//                     settings.MinAvgTurnoverForAdditionByBox == newMinAvgTurnover), It.IsAny<CancellationToken>()),
//             Times.Exactly(3));
//         
//         // 4. Test NeedAddToDb
//         var initialNeedAddToDb = component.Instance.NeedAddToDb;
//         var newNeedAddToDb     = !initialNeedAddToDb;
//         
//         await component.InvokeAsync(() => component.Instance.NeedAddToDb = newNeedAddToDb);
//         
//         component.Instance.NeedAddToDb.Should().Be(newNeedAddToDb);
//         _localStorageMock.Verify(
//             ls => ls.SetItemAsync(It.IsAny<string>(),
//                 It.Is<BaseProductCalculatorDialog<,>.CalculatorSettingsData>(
//                     settings => settings.NeedAddToDb == newNeedAddToDb),
//                 It.IsAny<CancellationToken>()), Times.Exactly(4));
//     }
//     
//     
//     public async Task
//         Should_OpenManualOrderInputDialog_When_AverageTurnoverIsZeroAndAvailableQuantityIsGreaterThanZero()
//     {
//         // Arrange
//         Services.AddMudBlazorDialog();
//         var dialogService = Services.GetService<IDialogService>() as DialogService;
//         
//         var cut = RenderComponent<MudDialogProvider>();
//         
//         var mockDialogService = new Mock<IDialogService>();
//         
//         var productTableItem = new ProductTableItem
//         {
//             Article           = "12345",
//             Name              = "Test Product",
//             AverageTurnover   = 0.0,
//             AvailableQuantity = 100,
//             QuantityToOrder   = 0
//         };
//         
//         var productTableItems = new[] { productTableItem };
//         
//         _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
//         
//         // Setup mock for ManualOrderInputDialog
//         var manualDialogResult = DialogResult.Ok(true);
//         mockDialogService
//             .Setup(ds =>
//                 ds.Show<ManualInputDialog<ProductTableItem>>(It.IsAny<string>(), It.IsAny<DialogParameters>()))
//             .Returns(Mock.Of<IDialogReference>(dr => dr.Result == Task.FromResult(manualDialogResult)));
//         
//         await cut.InvokeAsync((async () =>
//             await dialogService!.ShowAsync<BaseProductCalculatorDialog<,>>("ProductCalculatorDialog",
//                 new DialogParameters { { "ProductTableItems", productTableItems } })));
//         cut.FindComponent<BaseProductCalculatorDialog<,>>().Instance.DialogService = mockDialogService.Object;
//         
//         // Act
//         cut.Find("#submit-calculation-button").Click();
//         
//         // Assert
//         mockDialogService.Verify(ds => ds.Show<ManualInputDialog<ProductTableItem>>(
//             It.IsAny<string>(), It.Is<DialogParameters>(p => p.Get<object>("Item") == productTableItem)), Times.Once);
//         productTableItem.QuantityToOrder.Should().Be(1);
//     }
//     
//     
//     public async Task
//         ShouldNot_OpenManualOrderInputDialog_When_AverageTurnoverMoreThanZeroAndAvailableQuantityIsGreaterThanZero()
//     {
//         // Arrange
//         Services.AddMudBlazorDialog();
//         var dialogService = Services.GetService<IDialogService>() as DialogService;
//         
//         var cut = RenderComponent<MudDialogProvider>();
//         
//         var mockDialogService = new Mock<IDialogService>();
//         
//         var productTableItem = new ProductTableItem
//         {
//             Article           = "12345",
//             Name              = "Test Product",
//             AverageTurnover   = 0.1,
//             AvailableQuantity = 100,
//             QuantityToOrder   = 0
//         };
//         
//         var productTableItems = new[] { productTableItem };
//         
//         _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
//         
//         await cut.InvokeAsync((async () =>
//             await dialogService!.ShowAsync<BaseProductCalculatorDialog<,>>("ProductCalculatorDialog",
//                 new DialogParameters { { "ProductTableItems", productTableItems } })));
//         cut.FindComponent<BaseProductCalculatorDialog<,>>().Instance.DialogService = mockDialogService.Object;
//         
//         // Act
//         cut.Find("#submit-calculation-button").Click();
//         
//         // Assert
//         mockDialogService.Verify(ds => ds.Show<ManualInputDialog<ProductTableItem>>(
//             It.IsAny<string>(), It.Is<DialogParameters>(p => p.Get<object>("Item") == productTableItem)), Times.Never);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Should_Not_Add_Product_To_Database_When_NeedAddToDb_IsFalse()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         
//         _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 1.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 0,
//                 StockDays         = 10
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = -1;
//         cut.Instance.NeedAddToDb                    = false;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
//         dialogServiceMock.Verify(ds => ds.ShowAsync<ProductFormDialog>(
//                 It.IsAny<string>(),
//                 It.Is<DialogParameters<ProductFormDialog>>(p => p.Get<Product>("EditedProduct").Article == "12345")),
//             Times.Never);
//         productTableItems[0].QuantityToOrder.Should().Be(60);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Should_Add_Product_To_Database_When_NeedAddToDb_IsTrue()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         var dialogReferenceMock = new Mock<IDialogReference>();
//         var dialogResult        = DialogResult.Ok(new Product { Article = "12345", Name = "Test Product" });
//         dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
//         dialogServiceMock.Setup(d =>
//                 d.ShowAsync<ProductFormDialog>(It.IsAny<string>(), It.IsAny<DialogParameters<ProductFormDialog>>()))
//             .ReturnsAsync(dialogReferenceMock.Object);
//         
//         _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 1.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 0,
//                 StockDays         = 10
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = -1;
//         cut.Instance.NeedAddToDb                    = true;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         dialogServiceMock.Verify(ds => ds.ShowAsync<ProductFormDialog>(
//                 It.IsAny<string>(),
//                 It.Is<DialogParameters<ProductFormDialog>>(p => p.Get<Product>("EditedProduct").Article == "12345")),
//             Times.Once);
//         productTableItems[0].QuantityToOrder.Should().Be(60);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Should_ShowManualInputDialog_When_AverageTurnoverIsZero()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         var dialogReferenceMock = new Mock<IDialogReference>();
//         var dialogResult        = DialogResult.Ok(true);
//         dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
//         dialogServiceMock.Setup(d =>
//                 d.Show<ManualInputDialog<ProductTableItem>>(It.IsAny<string>(),
//                     It.IsAny<DialogParameters<ManualInputDialog<ProductTableItem>>>()))
//             .Returns((string s, DialogParameters<ManualInputDialog<ProductTableItem>> p) =>
//             {
//                 p.Get<ProductTableItem>("Item").QuantityToOrder = 10;
//                 return dialogReferenceMock.Object;
//             });
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 0.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 0,
//                 StockDays         = 0
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = -1;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         dialogServiceMock.Verify(ds => ds.Show<ManualInputDialog<ProductTableItem>>(
//                 It.IsAny<string>(),
//                 It.Is<DialogParameters>(p => p.Get<ProductTableItem>("Item") == productTableItems[0])),
//             Times.Once);
//         productTableItems[0].QuantityToOrder.Should().Be(10);
//     }
//     
//     
//     [Fact]
//     public async Task CalculateProducts_Should_Continue_When_ManualInputDialog_Is_Canceled()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         var dialogReferenceMock = new Mock<IDialogReference>();
//         var dialogResult        = DialogResult.Cancel();
//         dialogReferenceMock.Setup(d => d.Result).ReturnsAsync(dialogResult);
//         dialogServiceMock.Setup(d =>
//                 d.Show<ManualInputDialog<ProductTableItem>>(It.IsAny<string>(),
//                     It.IsAny<DialogParameters<ManualInputDialog<ProductTableItem>>>()))
//             .Returns((string s, DialogParameters<ManualInputDialog<ProductTableItem>> p) =>
//                 dialogReferenceMock.Object);
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 0.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 0,
//                 StockDays         = 0
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = 0;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         dialogServiceMock.Verify(ds => ds.Show<ManualInputDialog<ProductTableItem>>(
//                 It.IsAny<string>(),
//                 It.Is<DialogParameters>(p => p.Get<ProductTableItem>("Item") == productTableItems[0])),
//             Times.Once);
//         productTableItems[0].QuantityToOrder.Should().Be(0);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Should_Not_ShowManualInputDialog_When_AverageTurnoverIsMoreThanZero()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 1.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 10,
//                 StockDays         = 10
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = -1;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         dialogServiceMock.Verify(ds => ds.Show<ManualInputDialog<ProductTableItem>>(
//                 It.IsAny<string>(),
//                 It.Is<DialogParameters>(p => p.Get<ProductTableItem>("Item") == productTableItems[0])),
//             Times.Never);
//         productTableItems[0].QuantityToOrder.Should().Be(60);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Should_CalculateByBox()
//     {
//         // Arrange
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "12345",
//                 Name              = "Test Product",
//                 AverageTurnover   = 1.0,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 10,
//                 StockDays         = 10,
//                 DbReference       = new Product { Article = "12345", Name = "Test Product", QuantityPerBox = 54 }
//             },
//             new ProductTableItem
//             {
//                 Article           = "67890",
//                 Name              = "Test Product 2",
//                 AverageTurnover   = 0.5,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 10,
//                 StockDays         = 10,
//                 DbReference       = new Product { Article = "67890", Name = "Test Product 2", QuantityPerBox = 54 }
//             },
//             new ProductTableItem
//             {
//                 Article           = "678901",
//                 Name              = "Test Product 3",
//                 AverageTurnover   = 1.4,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 10,
//                 StockDays         = 10,
//                 DbReference       = new Product { Article = "678901", Name = "Test Product 3", QuantityPerBox = 54 }
//             },
//             new ProductTableItem
//             {
//                 Article           = "6789011",
//                 Name              = "Test Product 4",
//                 AverageTurnover   = 0.2,
//                 AvailableQuantity = 100000,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 10,
//                 StockDays         = 10
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 60;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = .3;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         productTableItems[0].QuantityToOrder.Should().Be(54);
//         productTableItems[1].QuantityToOrder.Should().Be(54);
//         productTableItems[2].QuantityToOrder.Should().Be(108);
//         productTableItems[3].QuantityToOrder.Should().Be(12);
//     }
//     
//     [Fact]
//     public async Task CalculateProducts_Ver1()
//     {
//         // Arrange
//         Mock<IDialogService> dialogServiceMock = new Mock<IDialogService>();
//         Services.AddSingleton(dialogServiceMock.Object);
//         
//         var productTableItems = new List<ProductTableItem>
//         {
//             new ProductTableItem
//             {
//                 Article           = "40001732",
//                 Name              = "RETI AGE Anti-aging gel-cream",
//                 AverageTurnover   = 0.84,
//                 AvailableQuantity = 9609,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 7,
//                 StockDays         = 8.33,
//                 DbReference = new Product
//                     { Article = "40001732", Name = "RETI AGE Anti-aging gel-cream", QuantityPerBox = 54 }
//             },
//             new ProductTableItem
//             {
//                 Article           = "40007259",
//                 Name              = "SESMAHAL B3 Niacinamide",
//                 AverageTurnover   = 0.11,
//                 AvailableQuantity = 2767,
//                 QuantityToOrder   = 0,
//                 CurrentQuantity   = 1,
//                 StockDays         = 9.09
//             },
//             new ProductTableItem
//             {
//                 Article = "40007604",
//                 Name = "ПРОМОНАБОР SESDERMA",
//                 AverageTurnover = 0.18,
//                 AvailableQuantity = 111,
//                 QuantityToOrder = 0,
//                 CurrentQuantity = 1,
//                 StockDays = 5.56,
//                 DbReference = new Product { Article = "40007604", Name = "ПРОМОНАБОР SESDERMA", QuantityPerBox = 10 }
//             },
//         };
//         var cut = RenderComponent<BaseProductCalculatorDialog<,>>(builder =>
//             builder.Add(p => p.ProductTableItems, productTableItems));
//         
//         cut.Instance.DaysCount                      = 90;
//         cut.Instance.MinAvgTurnoverForAdditionByBox = .1;
//         
//         // Act
//         await cut.Instance.CalculateProducts();
//         
//         // Assert
//         productTableItems[0].QuantityToOrder.Should().Be(54);
//         productTableItems[1].QuantityToOrder.Should().Be(9);
//         productTableItems[2].QuantityToOrder.Should().Be(7);
//     }
// }

