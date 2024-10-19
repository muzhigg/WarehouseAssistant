using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

[Obsolete(
    "This class is deprecated and will be removed in a future version. Please use the ICalculationStrategy interface instead.")]
public sealed class OrderCalculator<TItem, TOptions>(
    ICalculationStrategy<TItem, TOptions> calculationStrategy,
    TOptions                              options)
    where TItem : ICalculatedTableItem where TOptions : ICalculationOptions
{
    public void CalculateOrderQuantity(TItem data)
    {
        calculationStrategy.CalculateQuantity(data, options);
    }
    
    public void CalculateOrderQuantity(IEnumerable<TItem> data)
    {
        foreach (TItem calculatedTableItem in data)
            CalculateOrderQuantity(calculatedTableItem);
    }
}