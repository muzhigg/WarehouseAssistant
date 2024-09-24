using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public sealed class ForNumberDaysCalculation : ICalculationStrategy<ProductTableItem>
{
    public int CalculateQuantity(ProductTableItem product, CalculationOptions options)
    {
        double result = product.AverageTurnover * options.DaysCount;
        
        if (options.ConsiderCurrentQuantity)
            result = Math.Max(0.0, result - product.CurrentQuantity);
        
        // Учитываем небольшую погрешность, чтобы избежать проблем с точностью
        result += 0.0001;
        
        product.QuantityToOrder = (int)Math.Floor(result);
        return product.QuantityToOrder;
    }
}