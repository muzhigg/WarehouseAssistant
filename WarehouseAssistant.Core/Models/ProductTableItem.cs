namespace WarehouseAssistant.Core.Models;

public class ProductTableItem
{
    public required string Name              { get; set; }
    public int    Article           { get; set; }
    public int    AvailableQuantity { get; set; }
    public int    CurrentQuantity   { get; set; }
    public int    Reserved          { get; set; }
    public double AverageTurnover   { get; set; }
    public double StockDays         { get; set; }
    public double OrderCalculation  { get; set; }
    public int    QuantityToOrder   { get; set; }
}