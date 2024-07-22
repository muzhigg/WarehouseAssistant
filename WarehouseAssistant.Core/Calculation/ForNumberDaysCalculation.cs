using WarehouseAssistant.Core.Models;

namespace WarehouseAssistant.Core.Calculation;

public class ForNumberDaysCalculation : ICalculationStrategy<ProductTableItem>
{
    public int CalculateQuantity(ProductTableItem product, CalculationOptions options)
    {
        double result          = product.AverageTurnover * options.DaysCount;

        if (options.ConsiderCurrentQuantity)
            result = Math.Max(0.0, result - product.CurrentQuantity);

        double minCanBeOrdered = product.AvailableQuantity * 0.07;

        if (result > minCanBeOrdered)
            result = minCanBeOrdered;

        return (int)Math.Floor(result);
    }
}