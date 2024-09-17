// ReSharper disable NullableWarningSuppressionIsUsed

using System.Diagnostics.CodeAnalysis;

namespace WarehouseAssistant.Data.Models;

public class Product
{
    [NotNull] public string? Article { get; set; } = null!;
    [NotNull] public string? Name    { get; set; } = null!;
    public long?  Barcode        { get; set; }
    public int?   QuantityPerBox { get; set; }
    public int?   QuantityPerShelf { get; set; }
}