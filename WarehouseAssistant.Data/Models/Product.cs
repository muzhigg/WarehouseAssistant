// ReSharper disable NullableWarningSuppressionIsUsed

namespace WarehouseAssistant.Data.Models;

public class Product
{
    public string Article { get; set; } = null!;
    public string Name    { get; set; } = null!;
    public long?  Barcode        { get; set; }
    public int?   QuantityPerBox { get; set; }
    public int?   QuantityPerShelf { get; set; }
}