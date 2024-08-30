// using System.Threading.Tasks;
// using MudBlazor;
// using WarehouseAssistant.WebUI.Dialogs;
//
// namespace WarehouseAssistant.WebUI.Tests.Dialogs;
//
// public partial class ManualOrderInputDialogTest : MudBlazorTestContext
// {
//     private ProductTableItemStub _productTableItemStub;
//
//     public ManualOrderInputDialogTest()
//     {
//         _productTableItemStub = new ProductTableItemStub()
//         {
//             Article   = "40001234",
//             Available = 100,
//             Name      = "Test Product",
//         };
//     }
//
//     [Fact]
//     public async Task ShouldOpenCorrect()
//     {
//         IRenderedComponent<MudDialogProvider> provider    = await OpenDialog();
//         var                                   textElement = provider.Find(".manual-order-input-text");
//         Assert.NotNull(textElement);
//
//         var additionalTextElement = provider.Find(".test-render-text");
//         Assert.NotNull(additionalTextElement);
//     }
//
//     [Fact]
//     public async Task ShouldChangeItem()
//     {
//         IRenderedComponent<MudDialogProvider> provider = await OpenDialog();
//
//         var inputElement = provider.FindComponent<MudTextField<uint>>();
//         Assert.NotNull(inputElement);
//         var input = inputElement.Find("input");
//         Assert.NotNull(input);
//         input.Change((uint)3);
//         input.Blur();
//         var submitButton = provider.Find(".manual-input-submit");
//         Assert.NotNull(submitButton);
//
//         submitButton.Click();
//
//         Assert.Equal(3, _productTableItemStub.QuantityToOrder);
//     }
//
//     private async Task<IRenderedComponent<MudDialogProvider>> OpenDialog()
//     {
//         IRenderedComponent<MudDialogProvider> provider = RenderedDialogProvider(out DialogService? service);
//
//         DialogParameters<ManualOrderInputDialog<ProductTableItemStub>> parameters =
//             new()
//             {
//                 { dialog => dialog.Item, _productTableItemStub },
//                 { dialog => dialog.Text, "Test text" },
//                 { dialog => dialog.AdditionalRenderFragment, RenderText }
//             };
//
//         await provider.InvokeAsync(() => service?.Show<ManualOrderInputDialog<ProductTableItemStub>>("T", parameters));
//
//         return provider;
//     }
// }