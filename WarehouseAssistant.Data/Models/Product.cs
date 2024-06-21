namespace WarehouseAssistant.Data.Models;

public class Product
{
    public string Article        { get; set; }
    public string Name           { get; set; }
    public long?  Barcode        { get; set; }
    public int?   QuantityPerBox { get; set; }
    public int?   QuantityPerShelf { get; set; }
}