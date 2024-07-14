using WarehouseAssistant.Core.Collections;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Services;
using Xunit.Abstractions;

namespace WarehouseAssistant.Core.Tests;

public class WorksheetLoaderTest(ITestOutputHelper output)
{
    [Fact]
    public void GetColumns_ShouldReturnCorrectCollection()
    {
        // Arrange
        using WorksheetLoader worksheetLoader = new(@"D:\Downloads\БГЛЦ 29.03.xlsx");

        // Act
        Dictionary<string, string?> columns = worksheetLoader.GetColumns();

        // Assert
        Assert.NotNull(columns);
        Assert.Equal(10, columns.Count);

        Assert.True(columns.ContainsKey("A"));
        Assert.Equal("Номенклатура", columns["A"]);

        Assert.True(columns.ContainsKey("B"));
        Assert.Equal("Артикул", columns["B"]);

        Assert.True(columns.ContainsKey("J"));
        Assert.Equal("Заказ на офис Спб", columns["J"]);
    }

    [Fact]
    public void ParseItems_ShouldReturnCorrectNumberOfItems()
    {
        using WorksheetLoader worksheetLoader = new(@"D:\Downloads\БГЛЦ 29.03.xlsx");
        ColumnMapping         columnMapping   = new ColumnMapping();
        columnMapping.AddMapping(ColumnMapping.NameKey, "A");
        columnMapping.AddMapping(ColumnMapping.ArticleKey, "B");
        columnMapping.AddMapping(ColumnMapping.AvailableQuantityKey, "D");
        columnMapping.AddMapping(ColumnMapping.CurrentQuantityKey, "E");
        columnMapping.AddMapping(ColumnMapping.ReservedKey, "F");
        columnMapping.AddMapping(ColumnMapping.AverageTurnoverKey, "G");
        columnMapping.AddMapping(ColumnMapping.StockDaysKey, "H");
        columnMapping.AddMapping(ColumnMapping.OrderCalculationKey, "I");
        List<ProductTableItem> list = worksheetLoader.ParseItems(columnMapping);

        Assert.NotNull(list);
        Assert.Equal(1051, list.Count);
    }

    [Fact]
    public void ParseItems_OutputFirst10()
    {
        using WorksheetLoader worksheetLoader = new(@"D:\Downloads\БГЛЦ 29.03.xlsx");
        ColumnMapping         columnMapping   = new ColumnMapping();
        columnMapping.AddMapping(ColumnMapping.NameKey, "A");
        columnMapping.AddMapping(ColumnMapping.ArticleKey, "B");
        columnMapping.AddMapping(ColumnMapping.AvailableQuantityKey, "D");
        columnMapping.AddMapping(ColumnMapping.CurrentQuantityKey, "E");
        columnMapping.AddMapping(ColumnMapping.ReservedKey, "F");
        columnMapping.AddMapping(ColumnMapping.AverageTurnoverKey, "G");
        columnMapping.AddMapping(ColumnMapping.StockDaysKey, "H");
        columnMapping.AddMapping(ColumnMapping.OrderCalculationKey, "I");
        List<ProductTableItem> list = worksheetLoader.ParseItems(columnMapping);

        int i = 1;
        foreach (ProductTableItem item in list.Take(10))
            output.WriteLine(
                $"Item {++i}: Name = {item.Name}, Article = {item.Article}, AvailableQuantity = {item.AvailableQuantity}, CurrentQuantity = {item.CurrentQuantity}, Reserved = {item.Reserved}, AverageTurnover = {item.AverageTurnover}, StockDays = {item.StockDays}, OrderCalculation = {item.OrderCalculation}");
    }

    [Fact]
    public void ParseItems_OutputLast10()
    {
        using WorksheetLoader worksheetLoader = new(@"D:\Downloads\БГЛЦ 29.03.xlsx");
        ColumnMapping         columnMapping   = new ColumnMapping();
        columnMapping.AddMapping(ColumnMapping.NameKey, "A");
        columnMapping.AddMapping(ColumnMapping.ArticleKey, "B");
        columnMapping.AddMapping(ColumnMapping.AvailableQuantityKey, "D");
        columnMapping.AddMapping(ColumnMapping.CurrentQuantityKey, "E");
        columnMapping.AddMapping(ColumnMapping.ReservedKey, "F");
        columnMapping.AddMapping(ColumnMapping.AverageTurnoverKey, "G");
        columnMapping.AddMapping(ColumnMapping.StockDaysKey, "H");
        columnMapping.AddMapping(ColumnMapping.OrderCalculationKey, "I");
        List<ProductTableItem> list = worksheetLoader.ParseItems(columnMapping);

        int i = 1;
        foreach (ProductTableItem item in list.TakeLast(10))
            output.WriteLine(
                $"Item {i++}: Name = {item.Name}, Article = {item.Article}, AvailableQuantity = {item.AvailableQuantity}, CurrentQuantity = {item.CurrentQuantity}, Reserved = {item.Reserved}, AverageTurnover = {item.AverageTurnover}, StockDays = {item.StockDays}, OrderCalculation = {item.OrderCalculation}");
    }
}