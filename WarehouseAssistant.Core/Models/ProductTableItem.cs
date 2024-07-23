using WarehouseAssistant.Core.Calculation;

namespace WarehouseAssistant.Core.Models;

public class ProductTableItem : ICalculationData
{
    private int _quantityToOrder;
    public required string Name              { get; set; }
    public required int    Article           { get; set; }
    public int    AvailableQuantity { get; set; }
    public int    CurrentQuantity   { get; set; }
    public int    Reserved          { get; set; }
    public double AverageTurnover   { get; set; }
    public double StockDays         { get; set; }
    public double OrderCalculation  { get; set; }

    public int QuantityToOrder
    {
        get => _quantityToOrder;
        set
        {
            int minCanBeOrdered = MinCanBeOrdered;
            _quantityToOrder = value > minCanBeOrdered ? minCanBeOrdered : value;
        }
    }

    private int MinCanBeOrdered => (int)Math.Floor(AvailableQuantity * 0.07);
}