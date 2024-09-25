using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.WebUI.Components;

public partial class ProductBoxesCounter : ComponentBase
{
    private double _boxesCount;
    private double _selectedBoxesCount;
    
    public void CountBoxes(ICollection<ProductTableItem> products)
    {
        _boxesCount = 0;
        foreach (ProductTableItem productTableItem in products)
        {
            if (productTableItem.QuantityToOrder == 0)
                continue;
            
            int perBox = productTableItem.DbReference?.QuantityPerBox ?? 54;
            
            _boxesCount += (double)productTableItem.QuantityToOrder / perBox;
        }
        
        StateHasChanged();
        Debug.WriteLine($"Boxes count: {_boxesCount}");
    }
    
    public void CountSelectedBoxes(ICollection<ProductTableItem> products)
    {
        _selectedBoxesCount = 0;
        foreach (ProductTableItem productTableItem in products)
        {
            if (productTableItem.QuantityToOrder == 0)
                continue;
            
            int perBox = productTableItem.DbReference?.QuantityPerBox ?? 54;
            
            _selectedBoxesCount += (double)productTableItem.QuantityToOrder / perBox;
        }
        
        StateHasChanged();
        Debug.WriteLine($"Selected boxes count: {_selectedBoxesCount}");
    }
}