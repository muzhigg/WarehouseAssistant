namespace WarehouseAssistant.Core.Calculation;

public class DaysBasedCalculationOptions : ICalculationOptions
{
    public int  DaysCount               { get; set; }
    public bool ConsiderCurrentQuantity { get; set; }
}