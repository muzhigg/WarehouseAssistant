using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.ProductOrderModule;

internal interface IOrderTableExportMethod
{
    public Dictionary<string, List<object>> Export(IEnumerable<ProductTableItem> productTableItems);
}