using System.Reflection;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Shared.Models;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WarehouseAssistant.Tests;

[Trait("Category", "Unit")]
public sealed class DaysBasedCalculationStrategyTests(ITestOutputHelper log)
{
    [Theory, Repeat(1000)]
    public void RandomTest()
    {
        int    expectedResult          = Random.Shared.Next(0, 1000);
        int    availableQuantity       = Random.Shared.Next(0, 10000);
        double averageTurnover         = expectedResult / 30.0;
        double stockDays               = availableQuantity / averageTurnover;
        int    currentQuantity         = Random.Shared.Next(0, 1000);
        bool   considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
        
        DaysBasedCalculationStrategy strategy = new DaysBasedCalculationStrategy();
        DaysBasedCalculationOptions options = new DaysBasedCalculationOptions()
        {
            ConsiderCurrentQuantity = considerCurrentQuantity,
            DaysCount               = 30
        };
        
        log.WriteLine(
            $"expectedResult: {expectedResult}, avgTurnover: {averageTurnover}, currentQuantity: {currentQuantity}, " +
            $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
            $"avaiableQuantity: {availableQuantity}");
        
        
        ProductTableItem productTableItem = new ProductTableItem
        {
            Article           = "40001234",
            AvailableQuantity = availableQuantity,
            AverageTurnover   = averageTurnover,
            StockDays         = stockDays,
            CurrentQuantity   = currentQuantity,
            Name              = "Product 1",
        };
        
        if (considerCurrentQuantity)
        {
            expectedResult = (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
        }
        
        if (availableQuantity * 0.07 < expectedResult)
        {
            expectedResult = (int)Math.Clamp(availableQuantity * 0.07, 0, int.MaxValue);
            log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
        }
        
        strategy.CalculateQuantity(productTableItem, options);
        
        Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    }
    
    //
    // [Theory]
    // [Repeat(1000)]
    // public void RandomTests()
    // {
    //     // Arrange
    //     int    expectedResult          = Random.Shared.Next(0, 1000);   //30
    //     double avgTurnover             = expectedResult / 30.0;         //2
    //     int    currentQuantity         = Random.Shared.Next(0, 1000);   //100
    //     double stockDays               = currentQuantity / avgTurnover; // 50
    //     bool   considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
    //     int    avaiableQuantity        = Random.Shared.Next(0, 10000); //1000
    //     log.WriteLine(
    //         $"expectedResult: {expectedResult}, avgTurnover: {avgTurnover}, currentQuantity: {currentQuantity}, " +
    //         $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
    //         $"avaiableQuantity: {avaiableQuantity}");
    //     
    //     if (considerCurrentQuantity)
    //     {
    //         expectedResult = expectedResult =
    //             (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
    //     }
    //     
    //     if (avaiableQuantity * 0.07 < expectedResult)
    //     {
    //         expectedResult = (int)Math.Clamp(avaiableQuantity * 0.07, 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
    //     }
    //     
    //     ProductTableItem productTableItem = new ProductTableItem
    //     {
    //         AverageTurnover   = avgTurnover,
    //         CurrentQuantity   = currentQuantity,
    //         AvailableQuantity = avaiableQuantity,
    //         StockDays         = stockDays
    //     };
    //     
    //     CalculationOptions calculationOptions = new CalculationOptions
    //     {
    //         DaysCount               = 30,
    //         ConsiderCurrentQuantity = considerCurrentQuantity
    //     };
    //     
    //     OrderCalculator<ProductTableItem> orderCalculator = new OrderCalculator<ProductTableItem>(
    //         new DaysBasedCalculationStrategy(),
    //         calculationOptions);
    //     
    //     // Act
    //     orderCalculator.CalculateOrderQuantity(productTableItem);
    //     
    //     // Assert
    //     Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    // }
    
    // [Theory]
    // [Repeat(1000)]
    // public void SimilarToRealRandomTests()
    // {
    //     // Arrange
    //     int    expectedResult          = Random.Shared.Next(0, 300);    //30
    //     double avgTurnover             = expectedResult / 30.0;         //2
    //     int    currentQuantity         = Random.Shared.Next(0, 450);    //100
    //     double stockDays               = currentQuantity / avgTurnover; // 50
    //     bool   considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
    //     int    avaiableQuantity        = Random.Shared.Next(0, 3420); //1000
    //     log.WriteLine(
    //         $"expectedResult: {expectedResult}, avgTurnover: {avgTurnover}, currentQuantity: {currentQuantity}, " +
    //         $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
    //         $"avaiableQuantity: {avaiableQuantity}");
    //     
    //     if (considerCurrentQuantity)
    //     {
    //         expectedResult = (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
    //     }
    //     
    //     if (avaiableQuantity * 0.07 < expectedResult)
    //     {
    //         expectedResult = (int)Math.Clamp(avaiableQuantity * 0.07, 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
    //     }
    //     
    //     ProductTableItem productTableItem = new ProductTableItem
    //     {
    //         AverageTurnover   = avgTurnover,
    //         CurrentQuantity   = currentQuantity,
    //         AvailableQuantity = avaiableQuantity,
    //         StockDays         = stockDays
    //     };
    //     
    //     CalculationOptions calculationOptions = new CalculationOptions
    //     {
    //         DaysCount               = 30,
    //         ConsiderCurrentQuantity = considerCurrentQuantity
    //     };
    //     
    //     OrderCalculator<ProductTableItem> orderCalculator = new OrderCalculator<ProductTableItem>(
    //         new DaysBasedCalculationStrategy(),
    //         calculationOptions);
    //     
    //     // Act
    //     orderCalculator.CalculateOrderQuantity(productTableItem);
    //     
    //     // Assert
    //     Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    // }
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