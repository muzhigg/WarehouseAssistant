using WarehouseAssistant.Core.Models;

namespace WarehouseAssistant.Core.Calculation;

public class ByRecommendedCalculation : ICalculationStrategy<ProductTableItem>
{
    public int CalculateQuantity(ProductTableItem product, CalculationOptions options)
    {
        if (product.OrderCalculation > 0)
            return 0;

        double result = Math.Abs(product.OrderCalculation);

        if (options.ConsiderCurrentQuantity)
            result = Math.Max(0.0, result - product.CurrentQuantity);

        double minCanBeOrdered = product.AvailableQuantity * 0.07;

        if (result > minCanBeOrdered)
            result = minCanBeOrdered;

        return (int)Math.Floor(result);
    }
}