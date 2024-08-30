using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace WarehouseAssistant.WebUI.Pages;

[UsedImplicitly]
[SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component.")]
public partial class ProductsDatabasePage : ComponentBase
{
    public bool InProgress
    {
        get => _inProgress;
        set
        {
            _inProgress       = value;
            if (_dataGrid != null) _dataGrid.Loading = value;
            else ShowError("DataGrid is not initialized.");
        }
    }
    
    [Inject] private IRepository<Product> Repository    { get; init; } = null!;
    [Inject] private ISnackbar            SnackBar      { get; set; }  = null!;
    [Inject] private IDialogService       DialogService { get; init; } = null!;
    
    private DataGrid<Product>?            _dataGrid;
    private readonly    ObservableCollection<Product> _products = [];
    private             string?                       _searchString;
    private             bool                          _inProgress;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await LoadProductsAsync();
    }
    
    private void ShowError(string message)
    {
        SnackBar?.Add(message, Severity.Error);
        Console.Error.WriteLine(message);
    }
    
    private async Task LoadProductsAsync()
    {
        InProgress = true;
        
        try
        {
            IEnumerable<Product>? products = await Repository.GetAllAsync();
            if (products == null)
                ShowError("Не удалось получить список продуктов.");
            else
                foreach (Product product in products)
                    _products.Add(product);
        }
        catch (HttpRequestException e)
        {
            ShowError($"Ошибка при получении списка продуктов: {e.StatusCode}");
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка при получении списка продуктов: {ex.Message}");
        }
        finally
        {
            InProgress = false;
            StateHasChanged();
        }
    }
    
    private bool FilterFunc(Product arg)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
        
        return arg.Article.Contains(_searchString, StringComparison.OrdinalIgnoreCase) ||
               arg.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task OnProductChanged(Product obj)
    {
        InProgress = true;
        try
        {
            await Repository.UpdateAsync(obj);
        }
        catch (HttpRequestException e)
        {
            ShowError($"Ошибка при обновлении продукта: {e.StatusCode}");
        }
        finally
        {
            InProgress = false;
            StateHasChanged();
        }
    }
    
    private async Task ShowAddProductDialog()
    {
        InProgress = true;
        IDialogReference? dialog = await DialogService.ShowAsync<AddDbProductDialog>("Добавить товар");
        DialogResult?     result = await dialog.Result;

        if (!result.Canceled)
        {
            Product product = (Product)result.Data;
            _products.Add(product);
        }

        InProgress = false;
        StateHasChanged();
    }

    private async Task DeleteItems(ICollection<Product> items)
    {
        InProgress = true;
        foreach (Product product in items)
        {
            try
            {
                await Repository.DeleteAsync(product.Article);
            }
            catch (HttpRequestException exception)
            {
                ShowError($"Не удалось удалить {product.Article}. {exception.StatusCode}");
                continue;
            }

            _products.Remove(product);
        }

        InProgress = false;
        StateHasChanged();
    }
    
    private void OnDeleteButtonClicked(MouseEventArgs obj)
    {
        if (_dataGrid == null)
        {
            ShowError("DataGrid is not initialized.");
            return;
        }
        
        _ = DeleteItems(_dataGrid.SelectedItems);
    }
}