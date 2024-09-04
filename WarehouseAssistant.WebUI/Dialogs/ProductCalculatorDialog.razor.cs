using System.Text;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Dialogs;

public sealed partial class ProductCalculatorDialog
{
    private const int DefaultPerBox = 54;
    
    [Inject] public   ISnackbar            Snackbar      { get; set; } = null!;
    [Inject] public   IRepository<Product> Repository    { get; set; } = null!;
    [Inject] internal IDialogService       DialogService { get; set; } = null!;
    [Inject] private  ILocalStorageService LocalStorage  { get; set; } = null!;
    
    [Parameter] public IEnumerable<ProductTableItem>? ProductTableItems { get; set; }
    
    [CascadingParameter] internal MudDialogInstance MudDialog { get; set; } = null!;
    
    private CalculationOptions _options = null!;
    
    private OrderCalculator<ProductTableItem> _calculator = null!;
    
    public int DaysCount
    {
        get => _settingsData.DaysCount;
        set
        {
            _settingsData.DaysCount = value;
            _options.DaysCount      = value;
            LocalStorage.SetItemAsync(LocalStorageKey, _settingsData).AndForget();
        }
    }
    
    public bool ConsiderCurrentQuantity
    {
        get => _settingsData.ConsiderCurrentQuantity;
        set
        {
            _settingsData.ConsiderCurrentQuantity = value;
            _options.ConsiderCurrentQuantity      = value;
            LocalStorage.SetItemAsync(LocalStorageKey, _settingsData).AndForget();
        }
    }
    
    public double MinAvgTurnoverForAdditionByBox
    {
        get => _settingsData.MinAvgTurnoverForAdditionByBox;
        set
        {
            _settingsData.MinAvgTurnoverForAdditionByBox = value;
            LocalStorage.SetItemAsync(LocalStorageKey, _settingsData).AndForget();
        }
    }
    
    public bool NeedAddToDb
    {
        get => _settingsData.NeedAddToDb;
        set
        {
            _settingsData.NeedAddToDb = value;
            LocalStorage.SetItemAsync(LocalStorageKey, _settingsData).AndForget();
        }
    }
    
    private CalculatorSettingsData _settingsData = null!;
    
    private const string LocalStorageKey = "CalculatorSettings";
    
    internal sealed class CalculatorSettingsData
    {
        public int    DaysCount                      { get; set; }
        public bool   ConsiderCurrentQuantity        { get; set; }
        public double MinAvgTurnoverForAdditionByBox { get; set; }
        public bool   NeedAddToDb                    { get; set; }
    }
    
    protected override async Task OnInitializedAsync()
    {
        _options    = new CalculationOptions();
        _calculator = new OrderCalculator<ProductTableItem>(new ForNumberDaysCalculation(), _options);
        
        _settingsData = await LocalStorage.GetItemAsync<CalculatorSettingsData>(LocalStorageKey) ??
                        new CalculatorSettingsData();
        
        _options.DaysCount               = _settingsData.DaysCount;
        _options.ConsiderCurrentQuantity = _settingsData.ConsiderCurrentQuantity;
        
        await base.OnInitializedAsync();
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        if (ProductTableItems != null && !ProductTableItems.Any())
        {
            Snackbar.Add("Нет товаров для расчёта", Severity.Error);
            MudDialog.Cancel();
        }
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
            foreach (ProductTableItem productTableItem in ProductTableItems)
            {
                Product? dbProduct =
                    dbProducts.FirstOrDefault(dbProduct => dbProduct.Article.Equals(productTableItem.Article));
                
                if (NeedAddToDb && dbProduct == null)
                    await AddToRepo(productTableItem, dbProducts);
                
                if (productTableItem is { AverageTurnover: <= 0.0, AvailableQuantity: > 0 })
                {
                    bool success = await ShowManualInputDialog(productTableItem, dbProduct);
                    
                    if (!success) continue;
                }
                else
                {
                    _calculator.CalculateOrderQuantity(productTableItem);
                }
                
                if (MinAvgTurnoverForAdditionByBox >= 0.0 &&
                    productTableItem.AverageTurnover >= MinAvgTurnoverForAdditionByBox)
                {
                    int    perBox      = dbProduct != null ? dbProduct.QuantityPerBox ?? DefaultPerBox : DefaultPerBox;
                    double boxQuantity = Math.Ceiling((double)productTableItem.QuantityToOrder / perBox);
                    
                    if (boxQuantity < 1.0)
                        boxQuantity = 1.0;
                    
                    productTableItem.QuantityToOrder = (int)(boxQuantity * perBox);
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
            stringBuilder.AppendLine($"Количество на коробку {dbProduct.QuantityPerBox}")
                .AppendLine($"Количество на полку {dbProduct.QuantityPerBox}");
        
        stringBuilder.AppendLine($"Максимальное количество: <b>{product.MaxCanBeOrdered}</b>");
        
        IDialogReference dialog =
            ManualOrderInputDialog<ProductTableItem>.Show(DialogService, product, stringBuilder.ToString());
        
        DialogResult? result = await dialog.Result;
        
        return !result.Canceled;
    }
    
    private async Task AddToRepo(ProductTableItem contextItem, List<Product> dbProducts)
    {
        Product? product = await ProductFormDialog.ShowAddDialogAsync(contextItem, DialogService);
        
        if (product != null)
            dbProducts.Add(product);
    }
    
    private void OnCancel(MouseEventArgs obj)
    {
        MudDialog.Cancel();
    }
}