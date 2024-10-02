using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.ProductOrder;

public partial class BaseProductCalculatorDialog<TStrategy, TOptions> : BaseProductCalculatorDialog
    where TStrategy : ICalculationStrategy<ProductTableItem, TOptions>, new()
    where TOptions : class, ICalculationOptions, new()
{
    protected virtual RenderFragment? ChildContent { get; init; }
    
    [CascadingParameter] internal MudDialogInstance MudDialog { get; set; } = null!;
    
    public bool NeedAddToDb { get; set; }
    
    protected TOptions Options { get; private set; } = new();
    
    protected TStrategy Strategy { get; init; } = new();
    
    protected override async Task OnParametersSetAsync()
    {
        TOptions? storageOpt =
            await LocalStorage.GetItemAsync<TOptions>($"{typeof(TStrategy).Name}_{typeof(TOptions).Name}_calc_opt");
        
        if (storageOpt != null) Options = storageOpt;
        
        StateHasChanged();
        
        await base.OnParametersSetAsync();
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        if (ProductTableItems != null && !ProductTableItems.Any())
        {
            Snackbar.Add("Нет товаров для расчёта", Severity.Error);
            MudDialog.Cancel();
        }
    }
    
    private async Task OnSubmit(MouseEventArgs obj)
    {
        await CalculateProducts();
        MudDialog.Close();
    }
    
    internal async Task CalculateProducts()
    {
        if (ProductTableItems != null)
            foreach (ProductTableItem productTableItem in ProductTableItems)
                await CalculateQuantity(productTableItem);
    }
    
    protected virtual async Task CalculateQuantity(ProductTableItem productTableItem)
    {
        if (NeedAddToDb && productTableItem.DbReference == null)
            productTableItem.DbReference =
                await ProductFormDialog.ShowAddDialogAsync(productTableItem, DialogService);
    }
    
    private void OnCancel(MouseEventArgs obj)
    {
        MudDialog.Cancel();
    }
}