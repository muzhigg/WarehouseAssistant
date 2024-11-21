using MudBlazor;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.DatabaseModule;

public class ProductFormDialogService(IDialogService dialogService) : IProductFormDialogService
{
    public async Task<bool> ShowAddDialogAsync(ProductTableItem productTableItem)
    {
        Product product = new()
        {
            Article = productTableItem.Article,
            Name    = productTableItem.Name,
        };
        
        bool success = await ShowAddDialogAsync(product);
        
        if (success)
            productTableItem.DbReference = product;
        
        return success;
    }
    
    public async Task<bool> ShowAddDialogAsync(Product product)
    {
        DialogParameters<ProductFormDialog> parameters = [];
        parameters.Add(productDialog => productDialog.EditedProduct, product);
        parameters.Add(productDialog => productDialog.IsEditMode, false);
        
        IDialogReference? dialog = await dialogService.ShowAsync<ProductFormDialog>("Добавить товар", parameters);
        DialogResult?     result = await dialog.Result;
        
        return result.Canceled == false || result.Data is true;
    }
    
    public async Task<Product?> ShowAddDialogAsync()
    {
        Product product = new();
        
        bool success = await ShowAddDialogAsync(product);
        
        return success ? product : null;
    }
    
    public async Task<bool> ShowEditDialogAsync(Product product)
    {
        DialogParameters<ProductFormDialog> parameters = [];
        
        parameters.Add(productDialog => productDialog.IsEditMode, true);
        parameters.Add(productDialog => productDialog.EditedProduct, product);
        
        IDialogReference? dialog =
            await dialogService.ShowAsync<ProductFormDialog>("Редактировать товар", parameters);
        DialogResult? result = await dialog.Result;
        
        if (result.Canceled)
            return false;
        
        return (bool)result.Data;
    }
}