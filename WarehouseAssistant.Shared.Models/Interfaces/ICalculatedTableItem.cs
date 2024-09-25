namespace WarehouseAssistant.Shared.Models;

public interface ICalculatedTableItem : ITableItem
{
    public int QuantityToOrder { get; set; }
    public int MaxCanBeOrdered { get; }
}