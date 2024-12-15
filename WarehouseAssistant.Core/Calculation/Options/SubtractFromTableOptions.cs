using System.Text.Json.Serialization;

namespace WarehouseAssistant.Core.Calculation;

public class SubtractFromTableOptions : ICalculationOptions
{
    public bool ConsiderCurrentQuantity { get; set; }
    
    [JsonIgnore] public List<OrderItem> OrderItems { get; set; } = [];
}