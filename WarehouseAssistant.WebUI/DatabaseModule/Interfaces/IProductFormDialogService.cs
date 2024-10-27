using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.DatabaseModule;

public interface IProductFormDialogService
{
    public Task<bool> ShowAddDialogAsync(ProductTableItem productTableItem);
    
    public Task<bool> ShowAddDialogAsync(Product product);
    
    public Task<Product?> ShowAddDialogAsync();
    
    public Task<bool> ShowEditDialogAsync(Product product);
}