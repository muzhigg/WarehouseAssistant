using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public interface ICalculationStrategy<in TCalculatedTableItem, in TCalculationOptions>
    where TCalculatedTableItem : ICalculatedTableItem
    where TCalculationOptions : ICalculationOptions
{
    void CalculateQuantity(TCalculatedTableItem data, TCalculationOptions opt);
}