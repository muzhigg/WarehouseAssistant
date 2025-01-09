using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using WarehouseAssistant.ProductOrderModule;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.Tests.ProductOrderModule.Export;

[TestSubject(typeof(DividedByBoxesTableExportMethod))]
public class DividedByBoxesTableExportMethodTest
{
    [Fact]
    public void Export_Should_Not_Add_When_Quantity_Is_Zero()
    {
        // Arrange
        List<ProductTableItem> tableItems = new List<ProductTableItem>()
        {
            new ProductTableItem()
            {
                Name              = "Product 1",
                Article           = "40000067",
                AvailableQuantity = 100000,
                QuantityToOrder   = 0,
                StockDays         = 20,
                DbReference       = new Product() { Name = "Product 1", Article = "40000067", QuantityPerBox = 4 }
            },
            new ProductTableItem()
            {
                Name              = "Product 2",
                Article           = "40000141",
                AvailableQuantity = 100000,
                QuantityToOrder   = 1,
                StockDays         = 19,
                DbReference       = new Product() { Name = "Product 2", Article = "40000141", QuantityPerBox = 52 }
            },
        };
        
        var exportMethod = new DividedByBoxesTableExportMethod(5);
        
        // Act
        var orders = exportMethod.Export(tableItems);
        
        // Assert
        orders.Should().HaveCount(1);
        orders.First().Value.Should().HaveCount(1);
    }
    
    [Fact]
    public void Export_ShouldReturnCorrectTable()
    {
        // Arrange
        var dbProducts = new List<Product>()
        {
            new Product() { Name = "Товар 1", Article = "40000067", QuantityPerBox = 4 },
            new Product() { Name = "Товар 2", Article = "40000141", QuantityPerBox = 52 },
            new Product() { Name = "Товар 3", Article = "40000658", QuantityPerBox = 6 },
            new Product() { Name = "Товар 4", Article = "40001457", QuantityPerBox = 29 },
            new Product() { Name = "Товар 5", Article = "40000063", QuantityPerBox = 7 },
            new Product() { Name = "Товар 6", Article = "40000068", QuantityPerBox = 8 },
            new Product() { Name = "Товар 7", Article = "40004036", QuantityPerBox = 11 },
            new Product() { Name = "Товар 8", Article = "40003920", QuantityPerBox = 37 },
        };
        
        var tableItems = new List<ProductTableItem>()
        {
            new ProductTableItem()
            {
                Name        = "Товар 1", Article = "40000067", AvailableQuantity = 100000, QuantityToOrder = 18,
                StockDays   = 20,
                DbReference = dbProducts.First(p => p.Article == "40000067")
            },
            new ProductTableItem()
            {
                Name        = "Товар 2", Article = "40000141", AvailableQuantity = 100000, QuantityToOrder = 19,
                StockDays   = 19,
                DbReference = dbProducts.First(p => p.Article == "40000141")
            },
            new ProductTableItem()
            {
                Name        = "Товар 3", Article = "40000658", AvailableQuantity = 100000, QuantityToOrder = 52,
                StockDays   = 23,
                DbReference = dbProducts.First(p => p.Article == "40000658")
            },
            new ProductTableItem()
            {
                Name        = "Товар 4", Article = "40001457", AvailableQuantity = 100000, QuantityToOrder = 9,
                StockDays   = 5,
                DbReference = dbProducts.First(p => p.Article == "40001457")
            },
            new ProductTableItem()
            {
                Name        = "Товар 5", Article = "40000063", AvailableQuantity = 100000, QuantityToOrder = 48,
                StockDays   = 26,
                DbReference = dbProducts.First(p => p.Article == "40000063")
            },
            new ProductTableItem()
            {
                Name        = "Товар 6", Article = "40000068", AvailableQuantity = 100000, QuantityToOrder = 60,
                StockDays   = 11,
                DbReference = dbProducts.First(p => p.Article == "40000068")
            },
            new ProductTableItem()
            {
                Name        = "Товар 7", Article = "40004036", AvailableQuantity = 100000, QuantityToOrder = 35,
                StockDays   = 29,
                DbReference = dbProducts.First(p => p.Article == "40004036")
            },
            new ProductTableItem()
            {
                Name        = "Товар 8", Article = "40003920", AvailableQuantity = 100000, QuantityToOrder = 108,
                StockDays   = 16,
                DbReference = dbProducts.First(p => p.Article == "40003920")
            },
        };
        
        var exportMethod = new DividedByBoxesTableExportMethod(5);
        
        // Act
        var orders = exportMethod.Export(tableItems);
        
        // Assert
        orders.Should().HaveCount(7);
        AssertQuantity(orders["Order 0"], 9, 36, 2);
        AssertQuantity(orders["Order 1"], 24, 71);
        AssertQuantity(orders["Order 2"], 37, 19, 14);
        AssertQuantity(orders["Order 3"], 4, 22);
        AssertQuantity(orders["Order 4"], 30);
        AssertQuantity(orders["Order 5"], 34);
        AssertQuantity(orders["Order 6"], 14, 33);
    }
    
    private void AssertQuantity(List<object> order, params int[] expectedQuantities)
    {
        order.Should().HaveCount(expectedQuantities.Length);
        for (var i = 0; i < expectedQuantities.Length; i++)
        {
            order[i].GetType().GetProperty("Quantity").GetValue(order[i]).Should().Be(expectedQuantities[i]);
        }
    }
}