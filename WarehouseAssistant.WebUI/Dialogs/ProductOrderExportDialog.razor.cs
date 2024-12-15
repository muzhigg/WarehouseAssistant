using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Services;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.Dialogs;

[UsedImplicitly]
public partial class ProductOrderExportDialog : ComponentBase
{
    private record Order(double BoxCount, List<OrderItem> Products)
    {
        public double          BoxCount { get; set; } = BoxCount;
        public List<OrderItem> Products { get; set; } = Products;
    }
    
    internal record OrderItem
    {
        public OrderItem(ProductTableItem product, Product? dbProduct)
        {
            Name    = product.Name;
            Article = product.Article;
            BoxSize = dbProduct?.QuantityPerBox ?? 54;
            
#if DEBUG
            AvailableQuantity     = product.AvailableQuantity;
            AverageTurnoverPerDay = product.AverageTurnover;
            StockDays             = product.StockDays;
#endif
        }
        
        [ExcelColumn(Name = "Название", Width = 50)]
        public string Name { get; set; }
        
        [ExcelColumn(Name = "Артикул")] public string Article { get; set; }
        
#if DEBUG
        [ExcelColumn(Name = "Доступное количество")]
        public int AvailableQuantity { get; set; }
        
        [ExcelColumn(Name = "Средний расход в день")]
        public double AverageTurnoverPerDay { get; set; }
        
        [ExcelColumn(Name = "Запас на кол-во дней")]
        public double StockDays { get; set; }
#endif
        
        [ExcelColumn(Name = "Количество")] public int Quantity { get; set; }
        [ExcelIgnore]                      public int BoxSize  { get; set; }
    }
    
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] public ISnackbar  Snackbar  { get; set; } = null!;
    
    [Parameter] public IEnumerable<ProductTableItem>? Products { get; set; }
    private            int                            _maxOrderSize = 20;
    
    protected override void OnInitialized()
    {
        MudDialog.Options.CloseButton          = true;
        MudDialog.Options.DisableBackdropClick = false;
        MudDialog.Options.CloseOnEscapeKey     = true;
        MudDialog.SetOptions(MudDialog.Options);
    }
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (Products is null)
        {
            Snackbar.Add("Нет данных для экспорта", Severity.Error);
            Cancel();
            return;
        }
        
        if (Products.All(p => p.QuantityToOrder == 0))
        {
            Snackbar.Add("Нет данных для экспорта", Severity.Error);
            Cancel();
        }
    }
    
    internal Dictionary<string, List<OrderItem>> DivideProductsIntoOrders(IEnumerable<ProductTableItem> products,
        int                                                                                             maxOrderSize)
    {
        products = products.Order(Comparer<ProductTableItem>.Create((a, b) => a.StockDays.CompareTo(b.StockDays)));
        
        var result = new List<Order> { new Order(0, []) };
        
        foreach (ProductTableItem tableItem in products)
        {
            OrderItem orderItem = new(tableItem, tableItem.DbReference);
            
            int remainingQuantity = tableItem.QuantityToOrder;
            int currentOrderIndex = 0;
            
            while (remainingQuantity > 0)
            {
                if (result.Count <= currentOrderIndex)
                    result.Add(new Order(0, new List<OrderItem>()));
                
                Order  order                 = result[currentOrderIndex];
                double availableSpaceInOrder = maxOrderSize - order.BoxCount;
                
                if (availableSpaceInOrder <= 0)
                {
                    currentOrderIndex++;
                    continue;
                }
                
                // Обработка дробной части
                remainingQuantity =
                    HandleFractionalPart(orderItem, order, remainingQuantity, ref availableSpaceInOrder);
                
                // Обработка целой части
                remainingQuantity = HandleWholePart(orderItem, order, remainingQuantity, availableSpaceInOrder);
                
                // Переход к следующему заказу, если текущий заполнен
                currentOrderIndex++;
            }
        }
        
        return result.ToDictionary(order => GetOrderKey(result.IndexOf(order)), order => order.Products);
        
        
        static string GetOrderKey(int index)
        {
            return $"Order {index}";
        }
    }
    
    private static int HandleFractionalPart(OrderItem item, Order order, int remainingQuantity,
        ref double                                    availableSpaceInOrder)
    {
        double productBoxSize          = (double)remainingQuantity / item.BoxSize;
        double fractionalPartOfProduct = productBoxSize % 1;
        
        if (fractionalPartOfProduct > 0 && fractionalPartOfProduct <= availableSpaceInOrder)
        {
            int fractionalQuantity = (int)Math.Round(fractionalPartOfProduct * item.BoxSize);
            AddProductToCurrentOrder(order, item, fractionalQuantity);
            remainingQuantity     -= fractionalQuantity;
            availableSpaceInOrder -= fractionalPartOfProduct;
        }
        
        return remainingQuantity;
    }
    
    private static int HandleWholePart(OrderItem item, Order order, int remainingQuantity, double availableSpaceInOrder)
    {
        double productBoxSize      = (double)remainingQuantity / item.BoxSize;
        double wholePartOfProduct  = Math.Floor(productBoxSize);
        double wholeLotOfFreeSpace = Math.Floor(availableSpaceInOrder);
        
        if (wholePartOfProduct > 0 && wholeLotOfFreeSpace > 0)
        {
            double boxesToAdd    = Math.Min(wholePartOfProduct, wholeLotOfFreeSpace);
            int    wholeQuantity = (int)Math.Round(boxesToAdd * item.BoxSize);
            AddProductToCurrentOrder(order, item, wholeQuantity);
            remainingQuantity -= wholeQuantity;
        }
        
        return remainingQuantity;
    }
    
    private static void AddProductToCurrentOrder(Order order, OrderItem item, int quantity)
    {
        var existingProduct = order.Products.FirstOrDefault(p => p.Article == item.Article);
        
        if (existingProduct is not null)
            existingProduct.Quantity += quantity;
        else
            order.Products.Add(item with { Quantity = quantity });
        
        order.BoxCount += (double)quantity / item.BoxSize;
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
    private async Task Export()
    {
        var orders = DivideProductsIntoOrders(Products!, _maxOrderSize);
        
        if (orders.Count != 0)
        {
            await using WorkbookBuilder<OrderItem> workbookBuilder = new();
            
            foreach (KeyValuePair<string, List<OrderItem>> order in orders)
            {
                workbookBuilder.CreateSheet(order.Key);
                workbookBuilder.AddRangeToSheet(order.Key, order.Value);
            }
            
            var    xls      = workbookBuilder.AsByteArray();
            string fileName = $"Order {DateTime.Now}.xlsx";
            
            await JsRuntime.InvokeVoidAsync("DownloadExcelFile", fileName, xls);
        }
        
        MudDialog.Close(DialogResult.Ok(true));
    }
}