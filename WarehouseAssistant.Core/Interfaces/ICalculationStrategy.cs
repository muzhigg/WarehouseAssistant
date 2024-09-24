using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public interface ICalculationStrategy<in T> where T : ICalculatedTableItem
{
    int CalculateQuantity(T data, CalculationOptions options);
}