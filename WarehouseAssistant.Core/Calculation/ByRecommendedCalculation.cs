using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public class ByRecommendedCalculation : ICalculationStrategy<ProductTableItem, ICalculationOptions>
{
    public void CalculateQuantity(ProductTableItem product, ICalculationOptions options)
    {
        if (product.OrderCalculation > 0)
            return;
        
        double result = Math.Abs(product.OrderCalculation);
        
        if (options.ConsiderCurrentQuantity)
            result = Math.Max(0.0, result - product.CurrentQuantity);
        
        product.QuantityToOrder = (int)Math.Floor(result);
    }
}