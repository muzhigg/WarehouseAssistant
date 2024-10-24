using System.Diagnostics.CodeAnalysis;

namespace WarehouseAssistant.Shared.Models.Db;

public class Product
{
    [NotNull] public string? Article          { get; set; }
    [NotNull] public string? Name             { get; set; }
    public           string? Barcode          { get; set; }
    public           int?    QuantityPerBox   { get; set; }
    public           int?    QuantityPerShelf { get; set; }
}