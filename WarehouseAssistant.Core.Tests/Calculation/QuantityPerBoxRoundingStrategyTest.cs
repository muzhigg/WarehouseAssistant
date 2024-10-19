using JetBrains.Annotations;
using Moq;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.Tests;
using Xunit.Abstractions;

namespace WarehouseAssistant.Core.Tests.Calculation;

[TestSubject(typeof(QuantityPerBoxRoundingStrategy))]
public class QuantityPerBoxRoundingStrategyTest(ITestOutputHelper log)
{
    private readonly QuantityPerBoxRoundingStrategy _strategy = new();
    
    [Theory, Repeat(1000)]
    public void CalculateQuantity_RandomTest()
    {
        // Arrange
        var random          = new Random();
        var quantityToOrder = random.Next(1, 100); // Random quantity to order
        var quantityPerBox  = random.Next(1, 10);  // Random quantity per box
        
        log.WriteLine($"Quantity to order: {quantityToOrder}, Quantity per box: {quantityPerBox}");
        
        var data = new ProductTableItem
        {
            AvailableQuantity = 100000,
            QuantityToOrder   = quantityToOrder,
            DbReference       = new Product() { QuantityPerBox = quantityPerBox }
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        var expectedResult =
            (int)Math.Round((double)quantityToOrder / quantityPerBox, 0, MidpointRounding.AwayFromZero) *
            quantityPerBox;
        if (expectedResult == 0)
        {
            expectedResult = quantityPerBox;
        }
        
        log.WriteLine($"Expected result: {expectedResult}");
        Assert.Equal(expectedResult, data.QuantityToOrder);
    }
    
    [Fact]
    public void CalculateQuantity_HappyPath()
    {
        // Arrange
        var data = new ProductTableItem
        {
            AvailableQuantity = 10000,
            QuantityToOrder   = 10,
            DbReference       = new Product() { QuantityPerBox = 3 } // normal case
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        Assert.Equal(9, data.QuantityToOrder); // 10 / 3 rounds to 4, result should be 4 * 3 = 12
    }
    
    [Fact]
    public void CalculateQuantity_QuantityPerBoxIsNull()
    {
        // Arrange
        var data = new ProductTableItem
        {
            AvailableQuantity = 100000,
            QuantityToOrder   = 10,
            DbReference       = new Product { QuantityPerBox = null } // edge case: QuantityPerBox is null
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        Assert.Equal(10, data.QuantityToOrder); // no change expected
    }
    
    [Fact]
    public void CalculateQuantity_QuantityPerBoxIsZero()
    {
        // Arrange
        var data = new ProductTableItem
        {
            AvailableQuantity = 100000,
            QuantityToOrder   = 10,
            DbReference       = new Product { QuantityPerBox = 0 } // edge case: QuantityPerBox is zero
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        Assert.Equal(10, data.QuantityToOrder); // no change expected
    }
    
    [Fact]
    public void CalculateQuantity_QuantityToOrderIsZero()
    {
        // Arrange
        var data = new ProductTableItem
        {
            AvailableQuantity = 100000,
            QuantityToOrder   = 0,
            DbReference       = new Product { QuantityPerBox = 3 } // edge case: QuantityToOrder is zero
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        Assert.Equal(3, data.QuantityToOrder); // no change expected
    }
    
    [Fact]
    public void CalculateQuantity_ResultLessThanOne()
    {
        // Arrange
        var data = new ProductTableItem
        {
            AvailableQuantity = 100000,
            QuantityToOrder   = 1,
            DbReference       = new Product { QuantityPerBox = 5 } // edge case: result rounds to less than one
        };
        
        var options = new Mock<ICalculationOptions>();
        
        // Act
        _strategy.CalculateQuantity(data, options.Object);
        
        // Assert
        Assert.Equal(5, data.QuantityToOrder); // result should be 1 * 5 = 5
    }
    
    // [Theory, Repeat(1000)]
    // public void RandomTest()
    // {
    //     // int    expectedResult          = Random.Shared.Next(0, 1000);
    //     // int    availableQuantity       = Random.Shared.Next(0, 10000);
    //     // double averageTurnover         = expectedResult / 30.0;
    //     // double stockDays               = availableQuantity / averageTurnover;
    //     // int    currentQuantity         = Random.Shared.Next(0, 1000);
    //     // bool   considerCurrentQuantity = Random.Shared.Next(0, 2) == 0;
    //     
    //     
    //     int expectedBoxes = Random.Shared.Next(0, 20);
    //     int perBox        = Random.Shared.Next(2, 100);
    //     int quantityToOrder = Random.Shared.Next(expectedBoxes * perBox, (expectedBoxes * perBox + perBox) - 1);
    //
    //     var strategy                = new QuantityPerBoxRoundingStrategy();
    //     var options = new CalculationOptions();
    //     
    //     log.WriteLine(
    //         $"expectedResult: {expectedBoxes == 0 ? }, avgTurnover: {averageTurnover}, currentQuantity: {currentQuantity}, " +
    //         $"stockDays: {stockDays}, considerCurrentQuantity: {considerCurrentQuantity}, " +
    //         $"avaiableQuantity: {availableQuantity}");
    //     
    //     
    //     ProductTableItem productTableItem = new ProductTableItem
    //     {
    //         Article           = "40001234",
    //         Name              = "Product 1",
    //         QuantityToOrder = quantityToOrder,
    //         DbReference = new Product()
    //         {
    //             Article = "40001234",
    //             Name = "Product 1",
    //             QuantityPerBox = expectedBoxes == 0 ? null : perBox
    //         }
    //     };
    //     
    //     if (considerCurrentQuantity)
    //     {
    //         expectedResult = (int)Math.Clamp(Math.Ceiling((double)(expectedResult - currentQuantity)), 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering currentQuantity: {expectedResult}");
    //     }
    //     
    //     if (availableQuantity * 0.07 < expectedResult)
    //     {
    //         expectedResult = (int)Math.Clamp(availableQuantity * 0.07, 0, int.MaxValue);
    //         log.WriteLine($"expectedResult after considering 7% limit: {expectedResult}");
    //     }
    //     
    //     strategy.CalculateQuantity(productTableItem, options);
    //     
    //     Assert.Equal(expectedResult, productTableItem.QuantityToOrder);
    // }
}