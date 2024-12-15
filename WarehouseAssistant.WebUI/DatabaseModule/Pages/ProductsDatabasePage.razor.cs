using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;

namespace WarehouseAssistant.WebUI.DatabaseModule.Pages;

[UsedImplicitly]
public partial class ProductsDatabasePage : ComponentBase
{
    public bool InProgress
    {
        get => _inProgress;
        private set
        {
            _inProgress = value;
            _dataGrid?.SetLoading(value);
        }
    }
    
    [Inject] private IRepository<Product>          Repository        { get; init; } = null!;
    [Inject] private ISnackbar                     SnackBar          { get; set; }  = null!;
    [Inject] private IProductFormDialogService     ProductFormDialog { get; init; } = null!;
    [Inject] private ILogger<ProductsDatabasePage> Logger            { get; init; } = null!;
    [Inject] private IDialogService                DialogService     { get; init; } = null!;
    
    private DataGrid<Product>? _dataGrid;
    private List<Product>      _products = [];
    private string?            _searchString;
    private bool               _inProgress;
    private bool               _hasErrorOnRefresh;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await RefreshProductsAsync();
    }
    
    private async Task RefreshProductsAsync()
    {
        _dataGrid?.SetLoading(true);
        
        try
        {
            _products = await Repository.GetAllAsync() ??
                        throw new InvalidOperationException("Could not get the list of products.");
            Logger.LogInformation("Got {Count} products from database.", _products.Count);
            _hasErrorOnRefresh = false;
        }
        catch (HttpRequestException e)
        {
            _hasErrorOnRefresh = true;
            Logger.LogError(e, "Error getting products from database: {StatusCode}", e.StatusCode);
            SnackBar.Add($"Ошибка при получении списка продуктов: {e.StatusCode}", Severity.Error);
        }
        catch (Exception ex)
        {
            _hasErrorOnRefresh = true;
            Logger.LogError(ex, "Error getting products from database: {Message}", ex.Message);
            SnackBar.Add($"Ошибка при получении списка продуктов: {ex.Message}", Severity.Error);
        }
        finally
        {
            _dataGrid?.SetLoading(false);
            StateHasChanged();
        }
    }
    
    private bool FilterFunc(Product arg)
    {
        return string.IsNullOrWhiteSpace(_searchString) || arg.MatchesSearchString(_searchString);
    }
    
    private async Task ShowAddProductDialogAsync()
    {
        Product? product = await ProductFormDialog.ShowAddDialogAsync();
        if (product != null)
        {
            _products.Add(product);
            StateHasChanged();
        }
    }
    
    private async Task ShowEditProductDialog(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        await ProductFormDialog.ShowEditDialogAsync(product);
        
        StateHasChanged();
    }
    
    private async Task DeleteItem(Product contextItem)
    {
        Logger.LogInformation("Attempting to delete product {Name}", contextItem.Name);
        
        bool? success = await DialogService.ShowMessageBox("Удаление товара",
            $"Вы действительно хотите удалить товар {contextItem.Name}?",
            "Да", "Нет");
        
        if (success is not true)
        {
            Logger.LogInformation("Deleting product {Name} was cancelled.", contextItem.Name);
            return;
        }
        
        InProgress = true;
        
        try
        {
            await Repository.DeleteAsync(contextItem);
            Logger.LogInformation("Product {Name} deleted from database.", contextItem.Name);
            _products.Remove(contextItem);
            SnackBar.Add($"Товар {contextItem.Name} удален", Severity.Success);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error deleting product {Name}: {Message}", contextItem.Name, e.Message);
            SnackBar.Add($"Ошибка при удалении товара: {e.Message}", Severity.Error);
        }
        finally
        {
            InProgress = false;
            StateHasChanged();
        }
    }
}