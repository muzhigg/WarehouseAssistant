﻿using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Pages;

public partial class ProductsCalculationPage : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private IRepository<Product> Repository { get; set; } = null!;
    private          List<Product>?       _dbProducts = [];

    private DataGrid<ProductTableItem> _dataGrid = null!;

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

    private async Task ShowAddToDbDialog(ProductTableItem contextItem)
    {
        _dataGrid.Loading = true;

        DialogParameters<AddDbProductDialog> parameters = [];
        parameters.Add(productDialog => productDialog.Article, contextItem.Article);
        parameters.Add(productDialog => productDialog.ProductName, contextItem.Name);
        IDialogReference? dialog = await DialogService.ShowAsync<AddDbProductDialog>("Добавить товар", parameters);
        DialogResult?     result = await dialog.Result;

        if (!result.Canceled)
        {
            Product product = (Product)result.Data;
            if (_dbProducts != null) _dbProducts.Add(product);
            else _dbProducts = await Repository.GetAllAsync() as List<Product>;
        }

        _dataGrid.Loading = false;
        StateHasChanged();
    }

    private async Task ShowFileUploadDialog(MouseEventArgs obj)
    {
        _dataGrid.Loading = true;
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
            _products         = (IEnumerable<ProductTableItem>)result.Data;
            _dbProducts       = await Repository.GetAllAsync() as List<Product>;
        }

        _dataGrid.Loading = false;
    }
}