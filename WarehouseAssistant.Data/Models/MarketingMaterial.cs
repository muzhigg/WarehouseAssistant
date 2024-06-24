// ReSharper disable NullableWarningSuppressionIsUsed

namespace WarehouseAssistant.Data.Models;

public class MarketingMaterial
{
    public string Article { get; set; } = null!;
    public string Name    { get; set; } = null!;
    public string[] PackArticles { get; set; } = null!;
}