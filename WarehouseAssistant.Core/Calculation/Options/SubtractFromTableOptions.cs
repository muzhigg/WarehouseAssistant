namespace WarehouseAssistant.Core.Calculation;

public class SubtractFromTableOptions : ICalculationOptions
{
    public bool ConsiderCurrentQuantity { get; set; }
    
    public List<OrderItem> OrderItems { get; set; } = new();
}

public class OrderItem { }