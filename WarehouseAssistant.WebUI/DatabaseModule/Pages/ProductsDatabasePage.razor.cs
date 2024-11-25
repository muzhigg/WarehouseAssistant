using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;

namespace WarehouseAssistant.WebUI.DatabaseModule;

[UsedImplicitly]
public partial class ProductsDatabasePage : ComponentBase
{
    public bool InProgress
    {
        get => _inProgress;
        set
        {
            _inProgress = value;
            _dataGrid?.SetLoading(value);
        }
    }
    
    [Inject] private IRepository<Product>      Repository        { get; init; } = null!;
    [Inject] private ISnackbar                 SnackBar          { get; set; }  = null!;
    [Inject] private IProductFormDialogService ProductFormDialog { get; init; } = null!;
    
    private          DataGrid<Product>?            _dataGrid;
    private readonly ObservableCollection<Product> _products = [];
    private          string?                       _searchString;
    private          bool                          _inProgress;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await LoadProductsAsync();
    }
    
    private async Task LoadProductsAsync()
    {
        InProgress = true;
        
        try
        {
            Debug.WriteLine("Loading products...");
            IEnumerable<Product>? products = await Repository.GetAllAsync();
            
            if (products == null)
            {
                SnackBar.Add("Не удалось получить список продуктов.", Severity.Error);
            }
            else
            {
                foreach (Product product in products)
                    _products.Add(product);
                
                Debug.WriteLine($"Loaded {products.Count()} products.");
            }
        }
        catch (HttpRequestException e)
        {
            SnackBar.Add($"Ошибка при получении списка продуктов: {e.StatusCode}", Severity.Error);
            Debug.WriteLine($"HttpRequestException: {e.Message}");
        }
        catch (Exception ex)
        {
            SnackBar.Add($"Ошибка при получении списка продуктов: {ex.Message}", Severity.Error);
            Debug.WriteLine($"Exception: {ex.Message}");
        }
        finally
        {
            InProgress = false;
            StateHasChanged();
            Debug.WriteLine("Finished loading products.");
        }
    }
    
    private bool FilterFunc(Product arg)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
        
        return arg.Article.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
               arg.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }
    
    internal async Task ShowAddProductDialog()
    {
        if (InProgress)
            return;
        
        Product? product = await ProductFormDialog.ShowAddDialogAsync();
        if (product != null)
            _products.Add(product);
        
        StateHasChanged();
    }
    
    private async Task ShowEditProductDialog(Product? product)
    {
        if (product == null || InProgress)
            return;
        
        await ProductFormDialog.ShowEditDialogAsync(product);
        
        StateHasChanged();
    }
    
    private async Task DeleteItems(ICollection<Product>? items)
    {
        if (InProgress)
            return;
        
        InProgress = true;
        
        if (items is { Count: > 0 })
        {
            try
            {
                Debug.WriteLine("DeleteItems: Deleting selected items...");
                await Repository.DeleteRangeAsync(items);
                Debug.WriteLine("DeleteItems: Selected items deleted.");
                foreach (Product product in items)
                {
                    _products.Remove(product);
                    Debug.WriteLine($"DeleteItems: Product {product.Article} removed from local collection.");
                }
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Forbidden)
            {
                SnackBar.Add("Не удалось удалить выбранные продукты. У вас недостаточно прав.", Severity.Error);
                Debug.WriteLine($"DeleteItems: Failed to delete selected items. Forbidden: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                SnackBar.Add($"Не удалось удалить выбранные продукты. {e.StatusCode}", Severity.Error);
                Debug.WriteLine($"DeleteItems: Failed to delete selected items. HttpRequestException: {e.Message}");
            }
        }
        
        InProgress = false;
        StateHasChanged();
    }
    
    private async Task OnDeleteButtonClicked(MouseEventArgs obj)
    {
        if (_dataGrid == null) return;
        
        Debug.WriteLine("OnDeleteButtonClicked: Button clicked. Deleting selected items...");
        await DeleteItems(_dataGrid.SelectedItems);
    }
}