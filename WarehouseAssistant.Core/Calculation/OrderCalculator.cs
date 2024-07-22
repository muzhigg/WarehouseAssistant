namespace WarehouseAssistant.Core.Calculation;

public class OrderCalculator<T>(ICalculationStrategy<T> calculationStrategy, CalculationOptions options)
    where T : ICalculationData
{
    public int CalculateOrderQuantity(T data)
    {
        return calculationStrategy.CalculateQuantity(data, options);
    }
}