using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;

#pragma warning disable BL0005

namespace WarehouseAssistant.WebUI.DatabaseModule.Pages;

public partial class ProductsDatabasePage : ComponentBase
{
    [Inject] private ProductRepository Repository    { get; set; } = null!;
    [Inject] private ISnackbar         SnackBar      { get; set; } = null!;
    [Inject] private IDialogService    DialogService { get; set; } = null!;

    private          DataGrid<Product>             _dataGrid = null!;
    private readonly ObservableCollection<Product> _products = [];
    private          string?                       _searchString;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dataGrid.Loading = true;
            IEnumerable<Product>? allAsync = await Repository.GetAllAsync();
            if (allAsync == null)
                SnackBar.Add("Ошибка при получении списка", Severity.Error);
            else
                foreach (Product product in allAsync)
                    _products.Add(product);
            _dataGrid.Loading = false;
            StateHasChanged();
        }
    }

    private bool FilterFunc(Product arg)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        if (arg.Article!.Contains(_searchString)) return true;

        if (arg.Name!.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    private async Task OnProductChanged(Product obj)
    {
        _dataGrid.Loading = true;
        try
        {
            await Repository.UpdateAsync(obj);
        }
        catch (HttpRequestException e)
        {
            SnackBar.Add(e.StatusCode.ToString(), Severity.Error);
        }

        _dataGrid.Loading = false;
    }

    private async Task AddItem()
    {
        _dataGrid.Loading = true;
        IDialogReference? dialog = await DialogService.ShowAsync<AddDbProductDialog>("Добавить товар");
        DialogResult?     result = await dialog.Result;

        if (!result.Canceled)
        {
            Product product = (Product)result.Data;
            _products.Add(product);
        }

        _dataGrid.Loading = false;
        StateHasChanged();
    }

    private async Task DeleteItems()
    {
        _dataGrid.Loading = true;
        foreach (Product product in _dataGrid.SelectedItems)
        {
            try
            {
                await Repository.DeleteAsync(product.Article);
            }
            catch (HttpRequestException exception)
            {
                SnackBar.Add($"Не удалось удалить {product.Article}. {exception.StatusCode}");
                continue;
            }

            _products.Remove(product);
        }

        _dataGrid.Loading = false;
        StateHasChanged();
    }
}