using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Services;
using WarehouseAssistant.Data.Models;

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
        }
        
        [ExcelColumn(Name = "Название", Width = 50)]
        public string Name { get; set; }
        
        [ExcelColumn(Name = "Артикул")]    public string Article  { get; set; }
        [ExcelColumn(Name = "Количество")] public int    Quantity { get; set; }
        [ExcelIgnore]                      public int    BoxSize  { get; set; }
    }
    
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] public ISnackbar  Snackbar  { get; set; } = null!;
    
    [Parameter] public IEnumerable<ProductTableItem>? Products   { get; set; }
    [Parameter] public List<Product>?                 DbProducts { get; set; }
    private            int                            _maxOrderSize = 20;
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        if (Products is null)
        {
            Snackbar.Add("Нет данных для экспорта", Severity.Error);
            Cancel();
            return;
        }
    }
    
    internal Dictionary<string, List<OrderItem>> DivideProductsIntoOrders(IEnumerable<ProductTableItem> products,
        List<Product> dbProducts, int maxOrderSize)
    {
        products = products.Order(Comparer<ProductTableItem>.Create((a, b) => a.StockDays.CompareTo(b.StockDays)));
        
        var result = new List<Order> { new Order(0, []) };
        
        foreach (ProductTableItem tableItem in products)
        {
            OrderItem orderItem = new(tableItem, dbProducts?.FirstOrDefault(p => p.Article == tableItem.Article));
            
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
        {
            existingProduct.Quantity += quantity;
        }
        else
        {
            order.Products.Add(item with { Quantity = quantity });
        }
        
        order.BoxCount += (double)quantity / item.BoxSize;
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
    private async Task Export()
    {
        var orders = DivideProductsIntoOrders(Products!, DbProducts!, _maxOrderSize);
        
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