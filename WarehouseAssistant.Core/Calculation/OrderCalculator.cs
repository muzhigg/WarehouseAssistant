using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public sealed class OrderCalculator<T>(ICalculationStrategy<T> calculationStrategy, CalculationOptions options)
    where T : ICalculatedTableItem
{
    public int CalculateOrderQuantity(T data)
    {
        return calculationStrategy.CalculateQuantity(data, options);
    }
    
    public void CalculateOrderQuantity(IEnumerable<T> data)
    {
        foreach (T calculatedTableItem in data)
        {
            CalculateOrderQuantity(calculatedTableItem);
        }
    }
}