using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.WebUI.ProductOrderModule.Dialogs;
using WarehouseAssistant.WebUI.Tests;

namespace WarehouseAssistant.Core.Tests.Calculation;

public class SubtractFromTableStrategyDialogTests : MudBlazorTestContext
{
    private Mock<ISnackbar>            _snackbarMock            = new Mock<ISnackbar>();
    private Mock<IDialogService>       _dialogServiceMock       = new Mock<IDialogService>();
    private Mock<ILocalStorageService> _localStorageServiceMock = new Mock<ILocalStorageService>();
    
    public SubtractFromTableStrategyDialogTests()
    {
        Services.AddSingleton(_localStorageServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
    }
    
    [Fact]
    public async Task OnFilesChanged_Should_ParseMultipleFiles()
    {
        // Arrange
        List<ProductTableItem> tableItems = GetBaseTableItems();
        
        IRenderedComponent<MudDialogProvider> dialogProvider = RenderedDialog(tableItems);
        
        // Act
        var path  = @"../../../../samples/workbooks/Order .xlsx";
        var path2 = @"../../../../samples/workbooks/Order2.xlsx";
        dialogProvider.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(
                await File.ReadAllBytesAsync(path), "Order .xlsx",
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            InputFileContent.CreateFromBinary(
                await File.ReadAllBytesAsync(path2), "Order2.xlsx",
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        dialogProvider.Find("#submit-calculation-button").Click();
        
        // Assert
        tableItems[0].QuantityToOrder.Should().Be(0);
        tableItems[1].QuantityToOrder.Should().Be(10);
    }
    
    private IRenderedComponent<MudDialogProvider> RenderedDialog(List<ProductTableItem> tableItems)
    {
        Services.AddMudBlazorDialog();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        
        DialogParameters<SubtractFromTableStrategyDialog> parameters =
            new DialogParameters<SubtractFromTableStrategyDialog>
                { { dialog => dialog.ProductTableItems, tableItems } };
        
        dialogProvider.InvokeAsync(() =>
            Services.GetService<IDialogService>()!.ShowAsync<SubtractFromTableStrategyDialog>("", parameters));
        return dialogProvider;
    }
    
    private static List<ProductTableItem> GetBaseTableItems()
    {
        return
        [
            new ProductTableItem()
                { Name = "product1", Article = "1", AvailableQuantity = 10000, QuantityToOrder = 30 },
            
            new ProductTableItem()
                { Name = "product2", Article = "2", AvailableQuantity = 10000, QuantityToOrder = 30 }
        ];
    }
    
    [Fact]
    public async Task OnFilesChanged_Should_Should_HandleInvalidFiles()
    {
        // Arrange
        List<ProductTableItem>                tableItems     = GetBaseTableItems();
        IRenderedComponent<MudDialogProvider> dialogProvider = RenderedDialog(tableItems);
        
        // Act
        var path = @"../../../../samples/workbooks/товары 17.07.xlsx";
        dialogProvider.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(
            await File.ReadAllBytesAsync(path), "Order .xlsx",
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        dialogProvider.Find("#submit-calculation-button").Click();
        
        // Assert
        tableItems[0].QuantityToOrder.Should().Be(30);
        tableItems[1].QuantityToOrder.Should().Be(30);
    }
}