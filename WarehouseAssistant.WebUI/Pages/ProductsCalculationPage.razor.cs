using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Core.Models;

namespace WarehouseAssistant.WebUI.Pages;

public partial class ProductsCalculationPage : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private IEnumerable<ProductTableItem> _products     = new List<ProductTableItem>();
    private string                        _searchString = null!;

    private bool ShouldDisplayProduct(ProductTableItem arg)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        if (arg.Article.ToString().Contains(_searchString)) return true;

        if (arg.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    private void OnQuantityToOrderCommittedChanges(ProductTableItem obj)
    {
        if (obj.QuantityToOrder < 0)
        {
            Snackbar.Add("Значение не может быть меньше нуля", Severity.Error);
            obj.QuantityToOrder = 0;
        }
    }
}