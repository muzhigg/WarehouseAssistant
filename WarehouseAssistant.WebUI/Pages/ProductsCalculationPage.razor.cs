using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Pages;

public partial class ProductsCalculationPage : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Inject] private IDialogService DialogService { get; set; } = null!;

    private IEnumerable<ProductTableItem> _products     = new List<ProductTableItem>();
    private string                        _searchString = null!;

    private bool ShouldDisplayProduct(ProductTableItem arg)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        if (arg.Article!.Contains(_searchString)) return true;

        if (arg.Name!.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    private void OnQuantityToOrderCommittedChanges(ProductTableItem obj)
    {
        if (obj.QuantityToOrder < 0)
        {
            Snackbar.Add("Значение не может быть меньше нуля", Severity.Error);
            obj.QuantityToOrder = 0;
        }
    }

    private async Task ShowFileUploadDialog(MouseEventArgs obj)
    {
        DialogParameters<WorksheetUploadDialog<ProductTableItem>> parameters = [];

        parameters.Add(uploadDialog => uploadDialog.ExcelColumns, [
            new DynamicExcelColumn(nameof(ProductTableItem.Name)),
            new DynamicExcelColumn(nameof(ProductTableItem.AvailableQuantity)),
            new DynamicExcelColumn(nameof(ProductTableItem.OrderCalculation)),
            new DynamicExcelColumn(nameof(ProductTableItem.Article)),
            new DynamicExcelColumn(nameof(ProductTableItem.CurrentQuantity)),
            new DynamicExcelColumn(nameof(ProductTableItem.AverageTurnover)),
            new DynamicExcelColumn(nameof(ProductTableItem.StockDays)),
            new DynamicExcelColumn(nameof(ProductTableItem.QuantityToOrder)) { Ignore = true },
        ]);

        IDialogReference                                          dialog     = await DialogService.ShowAsync<WorksheetUploadDialog<ProductTableItem>>("Загрузка файла", parameters);
        DialogResult                                              result     = await dialog.Result;

        if (!result.Canceled)
        {
            IEnumerable<ProductTableItem> productItems = (IEnumerable<ProductTableItem>)result.Data;
            _products = productItems;
        }
    }
}