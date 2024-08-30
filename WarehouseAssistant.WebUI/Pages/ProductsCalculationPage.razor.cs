using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Dialogs;

[assembly: InternalsVisibleTo("WarehouseAssistant.WebUI.Tests")]

namespace WarehouseAssistant.WebUI.Pages;

[UsedImplicitly]
public partial class ProductsCalculationPage : ComponentBase
{
    public bool InProgress
    {
        get => _inProgress;
        private set
        {
            _inProgress = value;
            _dataGrid?.SetLoading(value);
            StateHasChanged();
        }
    }
    
    private int SelectedProductCount => _dataGrid?.SelectedItems.Count ?? 0;
    
    [Inject] private ISnackbar                     Snackbar      { get; set; } = null!;
    [Inject] private IDialogService                DialogService { get; set; } = null!;
    [Inject] private IRepository<Product>          Repository    { get; set; } = null!;
    
    internal IReadOnlyCollection<ProductTableItem> Products => _products.ToList().AsReadOnly();
    
    private List<Product>?                _dbProducts = [];
    private DataGrid<ProductTableItem>?   _dataGrid;
    private IEnumerable<ProductTableItem> _products     = new List<ProductTableItem>();
    private string                        _searchString = "";
    private bool                          _inProgress;
    
    protected override async Task OnInitializedAsync()
    {
        await RefreshDbProductListAsync();
        
#if DEBUG
        Console.WriteLine("ProductsCalculationPage.OnInitializedAsync() called");
#endif
    }
    
    private async Task RefreshDbProductListAsync()
    {
        try
        {
            _dbProducts = await Repository.GetAllAsync();
        }
        catch (Exception e)
        {
            string message = $"Не удалось получить список товаров из базы данных: {e.Message}";
            Snackbar.Add(message, Severity.Error);
            Console.WriteLine(message);
        }
    }
    
    private bool ShouldDisplayProduct(ProductTableItem arg)
    {
        return string.IsNullOrWhiteSpace(_searchString) ||
               arg.Article.Contains(_searchString) ||
               arg.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }
    
    internal async Task ShowAddToDbDialog(ProductTableItem contextItem)
    {
        InProgress = true;
        
        Product? product = await AddDbProductDialog.Show(contextItem, DialogService);
        
        if (product != null)
        {
            _dbProducts ??= [];
            _dbProducts.Add(product);
            Snackbar.Add("Товар успешно добавлен в базу данных", Severity.Success);
            // if (_dbProducts != null) _dbProducts.Add(product);
            // else _dbProducts = await Repository.GetAllAsync();
        }
        
        InProgress = false;
        // StateHasChanged();
    }
    
    public async Task ShowFileUploadDialogAsync()
    {
        InProgress = true;
        
        DialogParameters<WorksheetUploadDialog<ProductTableItem>> parameters = [];
        parameters.Add(uploadDialog => uploadDialog.ExcelColumns, [
            new DynamicExcelColumn(nameof(ProductTableItem.Name)),
            new DynamicExcelColumn(nameof(ProductTableItem.AvailableQuantity)),
            new DynamicExcelColumn(nameof(ProductTableItem.OrderCalculation)),
            new DynamicExcelColumn(nameof(ProductTableItem.Article)),
            new DynamicExcelColumn(nameof(ProductTableItem.CurrentQuantity)),
            new DynamicExcelColumn(nameof(ProductTableItem.AverageTurnover)),
            new DynamicExcelColumn(nameof(ProductTableItem.StockDays)),
            new DynamicExcelColumn(nameof(ProductTableItem.QuantityToOrder)) { Ignore = true }
        ]);
        
        IDialogReference dialog =
            await DialogService.ShowAsync<WorksheetUploadDialog<ProductTableItem>>("Загрузка файла", parameters);
        DialogResult result = await dialog.Result;
        
        if (!result.Canceled) _products   = (IEnumerable<ProductTableItem>)result.Data;
        // _dbProducts = await Repository.GetAllAsync();
        InProgress = false;
        // StateHasChanged();
    }
    
    public async Task ShowCalculatorDialog()
    {
        if (_dataGrid!.SelectedItems.Count == 0)
        {
            Snackbar.Add("Не выбрано ни одного элемента", Severity.Error);
            return;
        }
        
        InProgress = true;
        
        DialogParameters<ProductCalculatorDialog> parameters = [];
        parameters.Add(dialog => dialog.ProductTableItems, _dataGrid.SelectedItems);
        
        IDialogReference dialog = await DialogService.ShowAsync<ProductCalculatorDialog>("Расчет заказа", parameters);
        DialogResult     result = await dialog.Result;
        
        if (!result.Canceled && result.Data is true) 
            await RefreshDbProductListAsync();
        
        InProgress = false;
        // StateHasChanged();
    }
}