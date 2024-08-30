﻿using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Dialogs;

public partial class ProductCalculatorDialog
{
    private const int DefaultPerBox = 54;

    [Inject] public  ISnackbar            Snackbar      { get; set; }
    [Inject] public  IRepository<Product> Repository    { get; set; }
    [Inject] private IDialogService       DialogService { get; set; } = null!;


    [Parameter] public IEnumerable<ProductTableItem>? ProductTableItems { get; set; }

    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

    private CalculationOptions                _options    = null!;
    private OrderCalculator<ProductTableItem> _calculator = null!;

    private double              _minAvgTurnoverForAdditionByBox;
    private bool                _needAddToDb;

    private bool                _manualInputIsVisible;
    private MudDialog           _manualInputDialog;
    private MudTextField<uint>  _manualInputField;
    private MudText             _maxQuantityCanBeOrderedText;
    private CascadingValue<int> _casc;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // TODO get from localstorage

        _options    = new CalculationOptions();
        _calculator = new OrderCalculator<ProductTableItem>(new ForNumberDaysCalculation(), _options);
    }

    private async Task OnSubmit(MouseEventArgs obj)
    {
        await CalculateProducts();
        MudDialog.Close();
    }

    private async Task CalculateProducts()
    {
        List<Product> dbProducts = await Repository.GetAllAsync() ?? [];

        if (ProductTableItems != null)
        {
            foreach (ProductTableItem productTableItem in ProductTableItems)
            {
                Product? dbProduct =
                    dbProducts.FirstOrDefault(dbProduct => dbProduct.Article!.Equals(productTableItem.Article));

                // not tested
                if (_needAddToDb && dbProduct == null)
                {
                    await AddToRepo(productTableItem, dbProducts);
                }

                if (productTableItem is { AverageTurnover: <= 0.0, AvailableQuantity: > 0 })
                {
                    bool success = await ShowManualInputDialog(productTableItem, dbProduct);

                    if (!success) continue;
                }
                else
                {
                    _calculator.CalculateOrderQuantity(productTableItem);
                }

                if (_minAvgTurnoverForAdditionByBox >= 0.0 && productTableItem.AverageTurnover >= _minAvgTurnoverForAdditionByBox)
                {
                    int    perBox      = dbProduct != null ? dbProduct.QuantityPerBox ?? DefaultPerBox : DefaultPerBox;
                    double boxQuantity = Math.Ceiling((double)productTableItem.QuantityToOrder / perBox);

                    if (boxQuantity < 1.0)
                    {
                        boxQuantity = 1.0;
                    }

                    productTableItem.QuantityToOrder = (int)(boxQuantity * perBox);
                }
            }
        }
    }

    private async Task<bool> ShowManualInputDialog(ProductTableItem product, Product? dbProduct)
    {
        StringBuilder stringBuilder =
            new StringBuilder("Средняя оборачиваемость равна нулю. Требуется ручной ввод")
                .AppendLine($"Товар: {product.Name}")
                .AppendLine($"Артикул: {product.Article}");

        if (dbProduct != null)
        {
            stringBuilder.AppendLine($"Количество на коробку {dbProduct.QuantityPerBox}")
                .AppendLine($"Количество на полку {dbProduct.QuantityPerBox}");
        }

        stringBuilder.AppendLine($"Максимальное количество: <b>{product.MaxCanBeOrdered}</b>");

        DialogParameters<ManualOrderInputDialog<ProductTableItem>> parameters = new()
        {
            { dialog => dialog.AdditionalRenderFragment, ManualInputText(dbProduct) },
            { dialog => dialog.Item, product },
            { dialog => dialog.Text, stringBuilder.ToString() }
        };

        var dialog = await DialogService.ShowAsync<ManualOrderInputDialog<ProductTableItem>>("Введите количество", parameters);
        var result = await dialog.Result;

        if (result.Canceled || _manualInputField.Value == 0) return false;

        return true;
    }

    private async Task AddToRepo(ProductTableItem contextItem, List<Product> dbProducts)
    {
        Product? product = await AddDbProductDialog.Show(contextItem, DialogService);

        if (product != null)
        {
            dbProducts.Add(product);
        }
    }

    private void OnCancel(MouseEventArgs obj)
    {
        MudDialog.Cancel();
    }
}