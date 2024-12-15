using Microsoft.AspNetCore.Components.Forms;
using MiniExcelLibs;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.WebUI.ProductOrder;
using WarehouseAssistant.WebUI.Utils;

namespace WarehouseAssistant.WebUI.ProductOrderModule.Dialogs;

public partial class
    SubtractFromTableStrategyDialog : BaseProductCalculatorDialog<SubtractFromTableStrategy, SubtractFromTableOptions>
{
    private async Task OnFilesChanged(IReadOnlyList<IBrowserFile>? files)
    {
        if (files == null)
            return;
        
        Options.OrderItems.Clear();
        
        foreach (IBrowserFile browserFile in files)
        {
            using var memoryStream = await browserFile.ConvertBrowserFileToMemoryStream();
            foreach (string sheetName in memoryStream.GetSheetNames())
                Options.OrderItems.AddRange(await memoryStream.QueryAsync<OrderItem>(sheetName, ExcelType.XLSX));
        }
    }
}