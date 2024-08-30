// using MudBlazor.Services;
// using System.Net.Http;
// using System.Threading.Tasks;
// using MudBlazor;
// using WarehouseAssistant.WebUI.Dialogs;
// using System;
// using System.Collections.Generic;
// using WarehouseAssistant.Core.Models;
// using WarehouseAssistant.WebUI.Components;
//
// namespace WarehouseAssistant.WebUI.Tests.Dialogs;
//
// public class ProductCalculatorDialogTest : TestContext
// {
//     public ProductCalculatorDialogTest()
//     {
//         JSInterop.Mode = JSRuntimeMode.Loose;
//         Services.AddMudServices();
//         Services.AddScoped(_ => new HttpClient());
//         Services.AddOptions();
//     }
//
//     private IRenderedComponent<MudDialogProvider> RenderedDialogProvider(out DialogService? service)
//     {
//         IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
//         service = Services.GetService<IDialogService>() as DialogService;
//         return provider;
//     }
//
//     [Fact]
//     public async Task ShouldOpenCorrect()
//     {
//         IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);
//         IDialogReference?                     dialogReference = null;
//
//         // open dialog
//         HashSet<ProductTableItem> selectedItems = new HashSet<ProductTableItem>()
//         {
//             new ProductTableItem()
//             {
//                 Name = "BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл",
//                 Article = "40000252",
//                 AvailableQuantity = 1585,
//                 CurrentQuantity = 3,
//                 AverageTurnover = 0.07,
//                 StockDays = 42.86,
//                 OrderCalculation = -0.64,
//             }
//         };
//         DialogParameters<ProductCalculatorDialog> parameters    = [];
//         parameters.Add(dialog => dialog.ProductTableItems, selectedItems);
//
//         await provider.InvokeAsync(() => dialogReference = service?.Show<WorksheetUploadDialog<ProductTableItemStub>>("Расчет заказа", parameters));
//         var dialogInstance = provider.FindComponent<MudDialogInstance>();
//         Assert.NotNull(dialogInstance);
//         Assert.True(dialogInstance.Instance.Title.Contains("Расчет заказа", StringComparison.OrdinalIgnoreCase));
//
//
//     }
// }