using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public class IncrementByPercentageStrategy : ICalculationStrategy<ProductTableItem, IncrementByPercentageOptions>
{
    public void CalculateQuantity(ProductTableItem data, IncrementByPercentageOptions opt)
    {
        data.QuantityToOrder = (int)Math.Round(data.QuantityToOrder * (1 + (opt.Percentage / 100.0)));
    }
}