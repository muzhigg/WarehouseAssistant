using System.Reflection;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Calculation;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WarehouseAssistant.Tests;

[Trait("Category", "Unit")]
public sealed class ForNumberDaysCalculationTests(ITestOutputHelper log)
{
    // доступно больше чем 7%
    // среднее больше чем 0
    // запас больше чем 30
    // 
    [Theory]
    [InlineData(2696, 191, 4.1, 20.9, 123, false)]
    public void CalculateForNumberDays_ReturnsCorrectResult(int availableQuantity, int currentQuantity, double averageTurnover, double stockDays, int expectedResult, bool considerCurrentQuantity)
    {
        // Arrange
        ProductTableItem productTableItem = new ProductTableItem
        {
            AvailableQuantity = availableQuantity, CurrentQuantity = currentQuantity, AverageTurnover = averageTurnover,
            StockDays         = stockDays
        };
        
        OrderCalculator<ProductTableItem> orderCalculator = new OrderCalculator<ProductTableItem>(
            new ForNumberDaysCalculation(),
            new CalculationOptions()
            {
                ConsiderCurrentQuantity = considerCurrentQuantity,
                DaysCount               = 30
            });
        
        // Act
        orderCalculator.CalculateOrderQuantity(productTableItem);
            
        // Assert
        Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    }
    
    [Theory]
    [Repeat(100)]
    public void RandomTests()
    {
        // Arrange
        int expectedResult = Random.Shared.Next(0, 1000); //30
        double avgTurnover = expectedResult / 30.0; //2
        int currentQuantity = Random.Shared.Next(0, 1000); //100
        double stockDays = currentQuantity / avgTurnover; // 50
        bool considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
        int avaiableQuantity = Random.Shared.Next(0, 10000); //1000
        log.WriteLine(
            $"expectedResult: {expectedResult}, avgTurnover: {avgTurnover}, currentQuantity: {currentQuantity}, " +
            $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
            $"avaiableQuantity: {avaiableQuantity}");
        
        if (considerCurrentQuantity)
        {
            expectedResult = expectedResult = (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
        }
        
        if (avaiableQuantity * 0.07 < expectedResult)
        {
            expectedResult = (int)Math.Clamp(avaiableQuantity * 0.07, 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
        }
        
        ProductTableItem productTableItem = new ProductTableItem
        {
            AverageTurnover = avgTurnover,
            CurrentQuantity = currentQuantity,
            AvailableQuantity = avaiableQuantity,
            StockDays = stockDays
        };
        
        CalculationOptions calculationOptions = new CalculationOptions
        {
            DaysCount = 30,
            ConsiderCurrentQuantity = considerCurrentQuantity
        };
        
        OrderCalculator<ProductTableItem> orderCalculator = new OrderCalculator<ProductTableItem>(
            new ForNumberDaysCalculation(),
            calculationOptions);
        
        // Act
        orderCalculator.CalculateOrderQuantity(productTableItem);
        
        // Assert
        Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    } 
    
    [Theory]
    [Repeat(100)]
    public void SimilarToRealRandomTests()
    {
        // Arrange
        int expectedResult = Random.Shared.Next(0, 300); //30
        double avgTurnover = expectedResult / 30.0; //2
        int currentQuantity = Random.Shared.Next(0, 450); //100
        double stockDays = currentQuantity / avgTurnover; // 50
        bool considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
        int avaiableQuantity = Random.Shared.Next(0, 3420); //1000
        log.WriteLine(
            $"expectedResult: {expectedResult}, avgTurnover: {avgTurnover}, currentQuantity: {currentQuantity}, " +
            $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
            $"avaiableQuantity: {avaiableQuantity}");
        
        if (considerCurrentQuantity)
        {
            expectedResult = (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
        }
        
        if (avaiableQuantity * 0.07 < expectedResult)
        {
            expectedResult = (int)Math.Clamp(avaiableQuantity * 0.07, 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
        }
        
        ProductTableItem productTableItem = new ProductTableItem
        {
            AverageTurnover = avgTurnover,
            CurrentQuantity = currentQuantity,
            AvailableQuantity = avaiableQuantity,
            StockDays = stockDays
        };
        
        CalculationOptions calculationOptions = new CalculationOptions
        {
            DaysCount = 30,
            ConsiderCurrentQuantity = considerCurrentQuantity
        };
        
        OrderCalculator<ProductTableItem> orderCalculator = new OrderCalculator<ProductTableItem>(
            new ForNumberDaysCalculation(),
            calculationOptions);
        
        // Act
        orderCalculator.CalculateOrderQuantity(productTableItem);
        
        // Assert
        Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    }
}

public class RepeatAttribute : DataAttribute
{
    private readonly int _count;
    
    public RepeatAttribute(int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Repeat count must be greater than 0.");
        }
        _count = count;
    }
    
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return Enumerable.Repeat(new object[0], _count);
    }
}