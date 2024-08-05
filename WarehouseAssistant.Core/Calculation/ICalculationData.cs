namespace WarehouseAssistant.Core.Calculation;

public interface ICalculationData;

public interface ITableItem : ICalculationData
{
    public string? Name { get; set; }
    public string? Article { get; set; }

    public bool HasValidName();

    public bool HasValidArticle();
}