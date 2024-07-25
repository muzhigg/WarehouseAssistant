using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests;

public class WorkbookBuilderTests
{
    [Fact]
    public void SaveTest_ProductTableItem()
    {
        using WorkbookBuilder<ProductTableItem> workbookBuilder = GetWorkbookForProductTableItem();

        FillWorkbookWithProductTableItems(workbookBuilder);

        File.WriteAllBytes(@"D:\Downloads\TestWorkbook.xlsx", workbookBuilder.AsByteArray());
    }

    private static void FillWorkbookWithProductTableItems(WorkbookBuilder<ProductTableItem> workbookBuilder)
    {
        for (int i = 0; i < 10; i++)
        {
            ProductTableItem product = new()
            {
                Article           = i,
                AvailableQuantity = i + 1,
                AverageTurnover   = i + 0.1,
                CurrentQuantity   = i + 2,
                Name              = $"Product {i}",
                OrderCalculation  = i + 0.2,
                QuantityToOrder   = i + 3,
                StockDays         = i + 0.3
            };

            workbookBuilder.AddToSheet("Sheet 1", product);
        }

        for (int i = 9; i >= 0; i--)
        {
            ProductTableItem product = new()
            {
                Article           = i,
                AvailableQuantity = i + 1,
                AverageTurnover   = i + 0.1,
                CurrentQuantity   = i + 2,
                Name              = $"Product {i}",
                OrderCalculation  = i + 0.2,
                QuantityToOrder   = i + 3,
                StockDays         = i + 0.3
            };

            workbookBuilder.AddToSheet("Test 2", product);
        }
    }

    private static WorkbookBuilder<ProductTableItem> GetWorkbookForProductTableItem()
    {
        WorkbookBuilder<ProductTableItem>? worksheetBuilder = null;
        try
        {
            worksheetBuilder = new WorkbookBuilder<ProductTableItem>();

            worksheetBuilder.CreateSheet("Sheet 1");
            worksheetBuilder.CreateSheet("Test 2");
            return worksheetBuilder;
        }
        catch
        {
            worksheetBuilder?.Dispose();
            throw;
        }
    }

    [Fact]
    public void SaveTest_WithDynamicColumns()
    {
        using WorkbookBuilder<ProductTableItem> workbookBuilder = GetWorkbookForProductTableItem();

        FillWorkbookWithProductTableItems(workbookBuilder);

        OpenXmlConfiguration configuration = new()
        {
            DynamicColumns =
            [
                new DynamicExcelColumn("AvailableQuantity") { Ignore = true },
                new DynamicExcelColumn("CurrentQuantity") { Ignore   = true },
                new DynamicExcelColumn("AverageTurnover") { Ignore   = true },
                new DynamicExcelColumn("StockDays") { Ignore         = true },
                new DynamicExcelColumn("OrderCalculation") { Ignore  = true }
            ]
        };

        File.WriteAllBytes(@"D:\Downloads\TestWorkbook 2.xlsx", workbookBuilder.AsByteArray(configuration));
    }

    [Fact]
    public void SaveTest_DynamicType()
    {
        using WorkbookBuilder<dynamic> workbookBuilder = new();
        workbookBuilder.CreateSheet("Sheet 1");
        workbookBuilder.CreateSheet("Test 2");

        for (int i = 0; i < 10; i++)
        {
            var data = new
            {
                Article         = i,
                Name            = $"Product {i}",
                QuantityToOrder = i + 3,
            };

            workbookBuilder.AddToSheet("Sheet 1", data);
        }

        for (int i = 0; i < 10; i++)
        {
            var data = new
            {
                Article         = i,
                Name            = $"Product {i}",
                QuantityToOrder = i + 3,
                PerBox = 54,
                Boxes = i
            };

            workbookBuilder.AddToSheet("Test 2", data);
        }

        OpenXmlConfiguration configuration = new()
        {
            DynamicColumns =
            [
                new DynamicExcelColumn("Article") { Name = "Артикул"},
                new DynamicExcelColumn("Name") { Name = "Название"},
                new DynamicExcelColumn("QuantityToOrder") { Name = "К заказу" },
                new DynamicExcelColumn("PerBox") { Name = "Количество на коробку" },
                new DynamicExcelColumn("Boxes") { Name = "Коробки" },
            ]
        };

        File.WriteAllBytes(@"D:\Downloads\TestWorkbook 3.xlsx", workbookBuilder.AsByteArray(configuration));
    }
}