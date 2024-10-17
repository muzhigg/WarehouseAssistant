using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public sealed class QuantityPerBoxRoundingStrategy : ICalculationStrategy<ProductTableItem, ICalculationOptions>
{
    public void CalculateQuantity(ProductTableItem data, ICalculationOptions opt)
    {
        if (data.DbReference?.QuantityPerBox is null or 0)
            return;
        
        // if (data.QuantityToOrder == 0)
        //     return;
        
        double result = Math.Round(data.QuantityToOrder / (double)data.DbReference.QuantityPerBox, 0,
            MidpointRounding.AwayFromZero);
        
        if (result < 1)
            result = 1;
        
        data.QuantityToOrder = (int)(result * data.DbReference.QuantityPerBox);
    }
}