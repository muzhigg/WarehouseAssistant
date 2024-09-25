using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

[TestSubject(typeof(ProductOrderExportDialog))]
public class ProductOrderExportDialogTest
{
    [Fact]
    public void DivideProductsIntoOrders_Should_Return_Correct_Orders_And_Products()
    {
        // Arrange
        var dialog = new ProductOrderExportDialog();
        var dbProducts = new List<Product>()
        {
            new Product() { Name = "Товар 1", Article = "40000067", QuantityPerBox = 84 },
            new Product() { Name = "Товар 2", Article = "40000141", QuantityPerBox = 40 },
            new Product() { Name = "Товар 3", Article = "40000658", QuantityPerBox = 40 },
            new Product() { Name = "Товар 4", Article = "40001457", QuantityPerBox = 40 },
            new Product() { Name = "Товар 5", Article = "40000063", QuantityPerBox = 48 },
            new Product() { Name = "Товар 6", Article = "40000068", QuantityPerBox = 84 },
            new Product() { Name = "Товар 7", Article = "40004036", QuantityPerBox = 54 },
            new Product() { Name = "Товар 8", Article = "40003920", QuantityPerBox = 60 },
        };
        
        var tableItems = new List<ProductTableItem>()
        {
            new ProductTableItem()
            {
                Name = "Товар 1", Article = "40000067", AvailableQuantity = 100000, QuantityToOrder = 10, StockDays = 1,
                DbReference = dbProducts.First(p => p.Article == "40000067")
            },
            new ProductTableItem()
            {
                Name        = "Товар 2", Article = "40000141", AvailableQuantity = 100000, QuantityToOrder = 220,
                StockDays   = 2,
                DbReference = dbProducts.First(p => p.Article == "40000141")
            },
            new ProductTableItem()
            {
                Name        = "Товар 3", Article = "40000658", AvailableQuantity = 100000, QuantityToOrder = 245,
                StockDays   = 3,
                DbReference = dbProducts.First(p => p.Article == "40000658")
            },
            new ProductTableItem()
            {
                Name        = "Товар 4", Article = "40001457", AvailableQuantity = 100000, QuantityToOrder = 123,
                StockDays   = 4,
                DbReference = dbProducts.First(p => p.Article == "40001457")
            },
            new ProductTableItem()
            {
                Name = "Товар 5", Article = "40000063", AvailableQuantity = 100000, QuantityToOrder = 50, StockDays = 5,
                DbReference = dbProducts.First(p => p.Article == "40000063")
            },
            new ProductTableItem()
            {
                Name = "Товар 6", Article = "40000068", AvailableQuantity = 100000, QuantityToOrder = 26, StockDays = 6,
                DbReference = dbProducts.First(p => p.Article == "40000068")
            },
            new ProductTableItem()
            {
                Name = "Товар 7", Article = "40004036", AvailableQuantity = 100000, QuantityToOrder = 29, StockDays = 7,
                DbReference = dbProducts.First(p => p.Article == "40004036")
            },
            new ProductTableItem()
            {
                Name = "Товар 8", Article = "40003920", AvailableQuantity = 100000, QuantityToOrder = 60, StockDays = 8,
                DbReference = dbProducts.First(p => p.Article == "40003920")
            },
        };
        
        // Act
        var orders = dialog.DivideProductsIntoOrders(tableItems, 5);
        
        // Assert
        orders.Should().HaveCount(4);
        
        var orderItems = orders["Order 0"];
        orderItems.Should().HaveCount(5);
        orderItems.First(item => item.Article == tableItems[0].Article).Quantity.Should().Be(10);
        orderItems.First(item => item.Article == tableItems[1].Article).Quantity.Should().Be(180);
        orderItems.First(item => item.Article == tableItems[2].Article).Quantity.Should().Be(5);
        orderItems.First(item => item.Article == tableItems[3].Article).Quantity.Should().Be(3);
        orderItems.First(item => item.Article == tableItems[4].Article).Quantity.Should().Be(2);
        // orderItems.Should().Contain(item => item.Article == tableItems[0].Article && item.Quantity == 10);
        // orderItems.Should().Contain(item => item.Article == tableItems[1].Article && item.Quantity == 180);
        // orderItems.Should().Contain(item => item.Article == tableItems[2].Article && item.Quantity == 5);
        // orderItems.Should().Contain(item => item.Article == tableItems[3].Article && item.Quantity == 3);
        // orderItems.Should().Contain(item => item.Article == tableItems[4].Article && item.Quantity == 2);
    }
}