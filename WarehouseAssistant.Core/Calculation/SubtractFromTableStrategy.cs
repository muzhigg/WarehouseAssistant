using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.Core.Calculation;

public class SubtractFromTableStrategy : ICalculationStrategy<ProductTableItem, SubtractFromTableOptions>
{
    public void CalculateQuantity(ProductTableItem data, SubtractFromTableOptions opt)
    {
        var orderItems = opt.OrderItems.Where(item => item.Article == data.Article);
        foreach (var orderItem in orderItems)
        {
            data.QuantityToOrder -= orderItem.Quantity;
        }
    }
}