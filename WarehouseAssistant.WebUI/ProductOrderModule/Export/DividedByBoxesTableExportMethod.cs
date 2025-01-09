using MiniExcelLibs.Attributes;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.ProductOrderModule;

internal class DividedByBoxesTableExportMethod(int maxOrderSize) : IOrderTableExportMethod
{
    private record Order(double BoxCount, List<OrderItem> Products)
    {
        public double          BoxCount { get; set; } = BoxCount;
        public List<OrderItem> Products { get; set; } = Products;
    }
    
    private record OrderItem
    {
        public OrderItem(ProductTableItem product, Product? dbProduct)
        {
            Name              = product.Name;
            Article           = product.Article;
            AvailableQuantity = product.AvailableQuantity;
            CurrentQuantity   = product.CurrentQuantity;
            StockDays         = product.StockDays;
            BoxSize           = dbProduct?.QuantityPerBox ?? 54;
        }
        
        [ExcelColumn(Name = "Название", Width = 50)]
        public string Name { get; set; } = null!;
        
        [ExcelColumn(Name = "Артикул")] public string Article { get; set; } = null!;
        
        [ExcelColumn(Name = "Доступное количество")]
        public int AvailableQuantity { get; set; }
        
        [ExcelColumn(Name = "Текущее количество")]
        public int CurrentQuantity { get; set; }
        
        [ExcelColumn(Name = "Запас на кол-во дней")]
        public double StockDays { get; set; }
        
        [ExcelColumn(Name = "Количество")] public int Quantity { get; set; }
        [ExcelIgnore]                      public int BoxSize  { get; set; }
    }
    
    public Dictionary<string, List<object>> Export(IEnumerable<ProductTableItem> productTableItems)
    {
        List<Order> result = [];
        
        IEnumerable<ProductTableItem> tableItems = productTableItems.Where(item => item.QuantityToOrder != 0);
        
        if (tableItems.Any())
        {
            productTableItems = tableItems
                .Order(Comparer<ProductTableItem>.Create(
                    (a, b) => a.StockDays.CompareTo(b.StockDays)));
            
            result.Add(new Order(0, []));
            
            foreach (ProductTableItem tableItem in productTableItems)
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
        }
        
        return result.ToDictionary<Order, string, List<object>>(
            order => GetOrderKey(result.IndexOf(order)),
            order => order.Products.Cast<object>().ToList() // Преобразование в List<object>
        );
        
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
}