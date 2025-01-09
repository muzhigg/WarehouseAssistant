using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using WarehouseAssistant.Core.Services;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.ProductOrderModule;

[UsedImplicitly]
public partial class ProductOrderExportDialog : ComponentBase
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] public ISnackbar  Snackbar  { get; set; } = null!;
    
    [Parameter] public IEnumerable<ProductTableItem>? Products { get; set; }
    private            int                            _maxOrderSize = 20;
    private            bool                           _fullExport;
    
    protected override void OnInitialized()
    {
        MudDialog.Options.CloseButton          = true;
        MudDialog.Options.DisableBackdropClick = false;
        MudDialog.Options.CloseOnEscapeKey     = true;
        MudDialog.SetOptions(MudDialog.Options);
    }
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (Products is null)
        {
            Snackbar.Add("Нет данных для экспорта", Severity.Error);
            MudDialog.Cancel();
        }
    }
    
    private async Task Export()
    {
        var orders = ((IOrderTableExportMethod)(_fullExport
            ? new FullTableExportMethod()
            : new DividedByBoxesTableExportMethod(_maxOrderSize))).Export(Products!);
        
        if (orders.Count == 0)
        {
            Snackbar.Add("Нет данных для экспорта", Severity.Error);
            MudDialog.Cancel();
            return;
        }
        
        await using WorkbookBuilder<object> workbookBuilder = new();
        
        foreach (KeyValuePair<string, List<object>> order in orders)
        {
            workbookBuilder.CreateSheet(order.Key);
            workbookBuilder.AddRangeToSheet(order.Key, order.Value);
        }
        
        var    xls      = workbookBuilder.AsByteArray();
        string fileName = $"Order {DateTime.Now}.xlsx";
        
        await JsRuntime.InvokeVoidAsync("DownloadExcelFile", fileName, xls);
        
        MudDialog.Close(DialogResult.Ok(true));
    }
}