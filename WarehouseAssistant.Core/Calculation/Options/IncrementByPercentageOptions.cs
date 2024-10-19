namespace WarehouseAssistant.Core.Calculation;

public class IncrementByPercentageOptions : ICalculationOptions
{
    private double _percentage;
    public  bool   ConsiderCurrentQuantity { get; set; }
    
    public double Percentage
    {
        get => _percentage;
        set => _percentage = Math.Clamp(value, 0, 100);
    }
}