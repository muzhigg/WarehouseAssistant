using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public class SubtractCurrentQuantityCalculationStrategy : ICalculationStrategy<ProductTableItem, ICalculationOptions>
{
    public void CalculateQuantity(ProductTableItem data, ICalculationOptions opt)
    {
        if (data.QuantityToOrder == 0)
            return;
        
        data.QuantityToOrder -= data.CurrentQuantity;
    }
}