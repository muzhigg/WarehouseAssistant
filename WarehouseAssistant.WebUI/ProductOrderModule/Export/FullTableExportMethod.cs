using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.ProductOrderModule;

internal class FullTableExportMethod : IOrderTableExportMethod
{
    public Dictionary<string, List<object>> Export(IEnumerable<ProductTableItem> productTableItems)
    {
        var result = new Dictionary<string, List<object>> { { "Full Order", productTableItems.ToList<object>() } };
        return result;
    }
}