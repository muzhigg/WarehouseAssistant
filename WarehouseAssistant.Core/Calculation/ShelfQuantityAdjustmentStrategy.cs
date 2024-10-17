using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public sealed class ShelfQuantityAdjustmentStrategy : ICalculationStrategy<ProductTableItem, ICalculationOptions>
{
    public void CalculateQuantity(ProductTableItem data, ICalculationOptions opt)
    {
        if (data.DbReference?.QuantityPerShelf is null or 0) return;
        
        // if (data.QuantityToOrder == 0) return;
        
        if (!(data.QuantityToOrder < data.DbReference.QuantityPerShelf)) return;
        
        int result = (int)data.DbReference.QuantityPerShelf;
        
        if (opt.ConsiderCurrentQuantity)
            result = Math.Max(0, result - data.CurrentQuantity);
        
        data.QuantityToOrder = result;
    }
}