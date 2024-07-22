namespace WarehouseAssistant.Core.Calculation;

public interface ICalculationStrategy<in T> where T : ICalculationData
{
    int CalculateQuantity(T data, CalculationOptions options);
}