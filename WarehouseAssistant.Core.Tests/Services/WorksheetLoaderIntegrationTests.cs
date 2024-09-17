using System.ComponentModel;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Core.Services;
using Xunit.Abstractions;

namespace WarehouseAssistant.Core.Tests.Services;

[Trait("Category", "Integration")]
public sealed class WorksheetLoaderIntegrationTests(ITestOutputHelper log)
{
    private sealed class TableItemStub : ITableItem
    {
        [ExcelColumn(Name = "Название")]                         public string? Name        { get; set; }
        [ExcelColumn(Name = "Артикул")]                          public string? Article     { get; set; }

        [ExcelColumn(Name = "Кол-во", Aliases = ["Количество"])]
        public int     Amount      { get; set; }

        [ExcelColumn(Name = "Игнор")]                            public string? IgnoredProp { get; set; }

        public int NullInt { get; set; }

        public double MissingProp { get; set; }

        public int InvalidProp { get; set; }

        public bool    HasValidName()
        {
            return string.IsNullOrEmpty(Name) == false;
        }

        public bool    HasValidArticle()
        {
            return string.IsNullOrEmpty(Article) == false && Article.Length == 3;
        }
    }

    [Fact]
    public void Constructor_WithStream_ShouldInitialize()
    {
        // Arrange
        string       filePath     = GetEmptyWorkbookPath();
        FileStream   fileStream   = File.OpenRead(filePath);
        MemoryStream memoryStream = new MemoryStream();
        fileStream.CopyToAsync(memoryStream);

        // Act
        WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(memoryStream);

        // Assert
        Assert.NotNull(worksheetLoader);
    }

    private string GetEmptyWorkbookPath()
    {
        return GetPath("EmptyWorkbook");
    }

    private string GetPath(string workbookFileName)
    {
        var relativePath = $@"samples/workbooks/{workbookFileName}.xlsx";
        var basePath     = AppDomain.CurrentDomain.BaseDirectory;
        var filePath     = Path.Combine(basePath, @"..\..\..\..\", relativePath);
        filePath = Path.GetFullPath(filePath);
        return filePath;
    }

    [Fact]
    public void Constructor_WithPath_ShouldInitialize()
    {
        // Arrange
        string filePath = GetEmptyWorkbookPath();

        // Act
        WorksheetLoader<TableItemStub> worksheetLoader = new(filePath);

        // Assert
        Assert.NotNull(worksheetLoader);
    }

    [Fact]
    public void BaseParse_WithAttributes_ShouldReturnValidCollection()
    {
        // Arrange
        WorksheetLoader<TableItemStub> worksheetLoader = new(GetBaseWorkbookPath());

        // Act
        var result = worksheetLoader.ParseItems();

        // Assert
        var tableItemStubs = result.ToArray();
        Assert.Equal(8, tableItemStubs.Length);
        Assert.Equal("Товар 1", tableItemStubs[0].Name);
        Assert.Equal(1, tableItemStubs[0].Amount);
        Assert.Equal("Товар 9", tableItemStubs[7].Name);
        Assert.Equal(9, tableItemStubs[7].Amount);
    }

    private string GetBaseWorkbookPath()
    {
        return GetPath("BaseWorkbook");
    }

    [Fact]
    public void GetColumns_ShouldReturnValidCollection()
    {
        // Arrange
        WorksheetLoader<TableItemStub> worksheetLoader = new(GetBaseWorkbookPath());

        // Act
        var result = worksheetLoader.GetColumns();

        // Assert
        Assert.Equal("Артикул", result["A"]);
        Assert.Equal("Название", result["B"]);
        Assert.Equal("Количество", result["C"]);
    }

    [Fact]
    public void ParsingWithDynamicColumnSkipping()
    {
        // Arrange
        WorksheetLoader<TableItemStub> worksheetLoader = new(GetBaseWorkbookPath());
        DynamicExcelColumn[] conf = new[]
        {
            new DynamicExcelColumn(nameof(TableItemStub.IgnoredProp))
            {
                Ignore = true
            }
        };

        // Act
        TableItemStub[] result = worksheetLoader.ParseItems(conf).ToArray();

        // Assert
        Assert.True(result.All(stub => string.IsNullOrEmpty(stub.IgnoredProp)));
    }

    [Fact]
    public void ParsingColumnWithNullValues()
    {
        WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(GetBaseWorkbookPath());

        var result = worksheetLoader.ParseItems().ToArray();

        Assert.True(result.All(stub => stub.NullInt == default));
    }

    [Fact]
    public void HandlingMissingColumns()
    {
        WorksheetLoader<TableItemStub> worksheetLoader = new(GetBaseWorkbookPath());

        var result = worksheetLoader.ParseItems().ToArray();

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        Assert.True(result.All(stub => stub.MissingProp == default));
    }

    [Fact]
    public void HandlingIncorrectData()
    {
        WorksheetLoader<TableItemStub> worksheetLoader = new(GetBaseWorkbookPath());
        
        var result = worksheetLoader.ParseItems().ToList();
        
        Assert.Equal(1, result[0].InvalidProp);
        Assert.Equal(0, result[1].InvalidProp);
    }
}