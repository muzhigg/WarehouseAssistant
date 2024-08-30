using WarehouseAssistant.Core.Models;

namespace WarehouseAssistant.Core.Calculation;

public sealed class ForNumberDaysCalculation : ICalculationStrategy<ProductTableItem>
{
    public int CalculateQuantity(ProductTableItem product, CalculationOptions options)
    {
        double result          = product.AverageTurnover * options.DaysCount;

        if (options.ConsiderCurrentQuantity)
            result = Math.Max(0.0, result - product.CurrentQuantity);

        product.QuantityToOrder = (int)Math.Floor(result);
        return product.QuantityToOrder;
    }
}