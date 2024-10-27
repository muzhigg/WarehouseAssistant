using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.Dialogs;

[assembly: InternalsVisibleTo("WarehouseAssistant.WebUI.Tests")]

namespace WarehouseAssistant.WebUI.ProductOrder;

[UsedImplicitly]
public partial class ProductsCalculationPage : ComponentBase
{
    [Inject] private ISnackbar            Snackbar      { get; set; } = null!;
    [Inject] private IDialogService       DialogService { get; set; } = null!;
    [Inject] private IRepository<Product> Repository    { get; set; } = null!;
    
    private Table<ProductTableItem> _table = null!;
    
    private async Task RefreshProductsReferencesAsync(List<ProductTableItem> items)
    {
        try
        {
            var dbProducts = await Repository.GetAllAsync();
            if (dbProducts != null)
                foreach (ProductTableItem tableItem in items)
                    tableItem.DbReference = dbProducts.FirstOrDefault(p => p.Article == tableItem.Article);
        }
        catch (HttpRequestException e)
        {
            string message = $"Не удалось получить список товаров из базы данных: {e.Message}";
            Snackbar.Add(message, Severity.Error);
            Debug.WriteLine(message);
        }
    }
    
    private async Task ShowAddToDbDialog(ProductTableItem contextItem)
    {
        await ProductFormDialog.ShowAddDialogAsync(contextItem, DialogService);
    }
    
    private async Task ShowEditDbDialog(Product contextItem)
    {
        await ProductFormDialog.ShowEditDialogAsync(contextItem, DialogService);
    }
    
    private async Task OpenExportTableDialog()
    {
        DialogParameters<ProductOrderExportDialog> parameters = [];
        parameters.Add(dialog => dialog.Products, _table.SelectedItems);
        
        IDialogReference dialog =
            await DialogService.ShowAsync<ProductOrderExportDialog>("Экспорт заказов", parameters);
        await dialog.Result;
    }
    
    internal async Task OpenCalculationDialog<TDialog>()
        where TDialog : BaseProductCalculatorDialog
    {
        DialogParameters<TDialog> parameters = [];
        parameters.Add(dialog => dialog.ProductTableItems, _table.SelectedItems);
        
        IDialogReference dialog = DialogService.Show<TDialog>("dsa", parameters);
        await dialog.Result;
    }
    
    private async Task RemoveSelectedProductsAsync(MouseEventArgs obj)
    {
        bool? confirmed = await DialogService.ShowMessageBox("Внимание!",
            "Вы действительно хотите удалить выбранные товары?", "Удалить", "Отмена");
        
        if (confirmed == true)
        {
            await _table.RemoveSelectedItemsAsync();
            
            StateHasChanged();
        }
    }
    
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        
        Debug.WriteLine($"Items count: {_table.Items.Count}");
        Debug.WriteLine($"Selected items count: {_table.SelectedItems.Count}");
    }
}