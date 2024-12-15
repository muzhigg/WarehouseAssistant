using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public class SubtractFromTableStrategy : ICalculationStrategy<ProductTableItem, SubtractFromTableOptions>
{
    public void CalculateQuantity(ProductTableItem data, SubtractFromTableOptions opt)
    {
        throw new NotImplementedException();
    }
}