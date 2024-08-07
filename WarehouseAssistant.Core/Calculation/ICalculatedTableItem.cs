namespace WarehouseAssistant.Core.Calculation;

public interface ICalculatedTableItem : ITableItem
{
    public int QuantityToOrder { get; set; }
    public int MaxCanBeOrdered { get; }
}

public interface ITableItem
{
    public string? Name { get; set; }
    public string? Article { get; set; }

    public bool HasValidName();

    public bool HasValidArticle();
}