using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using WarehouseAssistant.ProductOrderModule;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.WebUI.Tests.ProductOrderModule.Export;

[TestSubject(typeof(FullTableExportMethod))]
public class FullTableExportMethodTest
{
    [Fact]
    public void Export_Should_Return_FullTable()
    {
        // Arrange
        FullTableExportMethod fullTableExportMethod = new FullTableExportMethod();
        List<ProductTableItem> productTableItems =
        [
            new ProductTableItem()
            {
                Article          = "40001234", AvailableQuantity = 10000,
                AverageTurnover  = 1.1, CurrentQuantity          = 5000, Name    = "Product 1",
                OrderCalculation = -53.1, QuantityToOrder        = 10, StockDays = 40.1
            },
            new ProductTableItem()
            {
                Article          = "40005678", AvailableQuantity = 20000,
                AverageTurnover  = 2.2, CurrentQuantity          = 10000, Name  = "Product 2",
                OrderCalculation = -106.2, QuantityToOrder       = 0, StockDays = 80.2
            }
        ];
        
        // Act
        var result = fullTableExportMethod.Export(productTableItems);
        
        // Assert
        result.Should().HaveCount(1);
        result.First().Value.Should().HaveCount(2);
        
        IEnumerable<ProductTableItem> cast = result.First().Value.Cast<ProductTableItem>();
        cast.Should().ContainEquivalentOf(productTableItems[0]);
        cast.Should().ContainEquivalentOf(productTableItems[1]);
    }
}