using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests;

public class WorksheetLoaderTests
{
    private Stream CreateFakeExcelStream(object rows)
    {
        return CreateFakeExcelStream(rows, false);
    }

    private Stream CreateFakeExcelStream(object rows, bool printHeader)
    {
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.SaveAs(rows, printHeader);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }

    [Fact]
    public void GetColumns_ShouldReturnDictionaryWithLength9AndNonEmptyValues()
    {
        // Arrange
        List<IDictionary<string, object>> data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "A", "Номенклатура" },
                { "B", "Артикул" },
                { "C", "C" },
                { "D", "D" },
                { "E", "E" },
                { "F", "F" },
                { "G", "G" },
                { "H", "H" },
                { "I", "I" }
            }
        };
        using Stream                         stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = worksheetLoader.GetColumns();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9, result.Count);
        Assert.All(result.Values, value => Assert.False(string.IsNullOrEmpty(value)));
    }

    [Fact]
    public void GetColumns_ShouldReturnEmptyDictionary()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        List<IDictionary<string, object>>    data            = new List<IDictionary<string, object>>();
        using Stream                         stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = worksheetLoader.GetColumns();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetColumns_ShouldReturnCorrectMappings()
    {
        // Arrange
        List<IDictionary<string, object>> data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "A", "Номенклатура" },
                { "B", "Артикул" },
                { "C", "C" },
                { "D", "D" },
                { "E", "E" },
                { "F", "F" },
                { "G", "Расчет заказа" },
                { "H", "H" },
                { "I", "I" }
            }
        };

        using Stream                         stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = worksheetLoader.GetColumns();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Номенклатура", result["A"]);
        Assert.Equal("Артикул", result["B"]);
        Assert.Equal("Расчет заказа", result["G"]);
    }

    [Fact]
    public async Task GetColumnsAsync_ShouldReturnDictionaryWithLength9AndNonEmptyValues()
    {
        // Arrange
        List<IDictionary<string, object>> data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "A", "Номенклатура" },
                { "B", "Артикул" },
                { "C", "C" },
                { "D", "D" },
                { "E", "E" },
                { "F", "F" },
                { "G", "Расчет заказа" },
                { "H", "H" },
                { "I", "I" }
            }
        };
        await using Stream                   stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = await worksheetLoader.GetColumnsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9, result.Count);
        Assert.All(result.Values, value => Assert.False(string.IsNullOrEmpty(value)));
    }

    [Fact]
    public async Task GetColumnsAsync_ShouldReturnEmptyDictionary()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        List<IDictionary<string, object>>    data            = new List<IDictionary<string, object>>();
        await using Stream                   stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = await worksheetLoader.GetColumnsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetColumnsAsync_ShouldReturnCorrectMappings()
    {
        // Arrange
        List<IDictionary<string, object>> data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "A", "Номенклатура" },
                { "B", "Артикул" },
                { "C", "C" },
                { "D", "D" },
                { "E", "E" },
                { "F", "F" },
                { "G", "Расчет заказа" },
                { "H", "H" },
                { "I", "I" }
            }
        };
        await using Stream                         stream          = CreateFakeExcelStream(data);
        await using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        // Act
        Dictionary<string, string?> result = await worksheetLoader.GetColumnsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Номенклатура", result["A"]);
        Assert.Equal("Артикул", result["B"]);
        Assert.Equal("Расчет заказа", result["G"]);
    }

    [Fact]
    public void ParseItems_ShouldReturnCorrectCollection()
    {
        // Arrange
        var data = new[]
        {
            new
            {
                A = "BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл",
                B = "40000252",
                C = "1585",
                D = "3",
                E = "0.07",
                F = "42.86",
                G = "-0.64"
            },
            new
            {
                A = "BTSES Wrinkle Inhibitor – Гель-ингибитор морщин, 15 мл",
                B = "40000250",
                C = "0",
                D = "51",
                E = "0.06",
                F = "850",
                G = "47.88"
            }
        };

        using Stream                                stream          = CreateFakeExcelStream(data, true);
        using WorksheetLoader<ProductTableItemStub> worksheetLoader = new WorksheetLoader<ProductTableItemStub>(stream);

        // Act
        var result = worksheetLoader.ParseItems().ToArray();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл", result[0].Name);
        Assert.Equal("40000250", result[1].Article);
        Assert.Equal(1585, result[0].C);
        Assert.Equal(0, result[0].D + result[1].D);
        Assert.Equal(0.07, result[0].E);
        Assert.Equal(0, result[1].Ff);
        Assert.Equal(0, result[0].G);
        Assert.Equal(47.88, result[1].G);
    }

    [Fact]
    public void ParseItems_ShouldReturnCorrectCollectionWithDynamicColumns()
    {
        // Arrange
        var data = new[]
        {
            new
            {
                Aa = "BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл",
                B = "40000252",
                C = "1585",
                D = "3",
                E = "0.07",
                F = "42.86",
                G = "-0.64"
            },
            new
            {
                Aa = "BTSES Wrinkle Inhibitor – Гель-ингибитор морщин, 15 мл",
                B = "40000250",
                C = "0",
                D = "51",
                E = "0.06",
                F = "850",
                G = "47.88"
            }
        };

        using Stream                                stream          = CreateFakeExcelStream(data, true);
        using WorksheetLoader<ProductTableItemStub> worksheetLoader = new(stream);

        DynamicExcelColumn[] columns =
        [
            new DynamicExcelColumn("Name") { Name   = "Aa" },
            new DynamicExcelColumn("C") { Ignore = true }
        ];

        // Act
        ProductTableItemStub[] result = worksheetLoader.ParseItems(columns).ToArray();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл", result[0].Name);
        Assert.Equal("40000250", result[1].Article);
        Assert.Equal(0, result[0].C);
        Assert.Equal(0, result[0].D);
        Assert.Equal(0.07, result[0].E);
        Assert.Equal(0, result[1].Ff);
        Assert.Equal(0, result[0].G);
        Assert.Equal(47.88, result[1].G);
    }

    [Fact]
    public void ParseItems_ShouldReturnEmptyCollection()
    {
        // Arrange
        // ReSharper disable once CollectionNeverUpdated.Local
        List<IDictionary<string, object>>    data            = new List<IDictionary<string, object>>();
        using Stream                         stream          = CreateFakeExcelStream(data);
        using WorksheetLoader<TableItemStub> worksheetLoader = new WorksheetLoader<TableItemStub>(stream);

        IEnumerable<TableItemStub> result = worksheetLoader.ParseItems();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

public class ProductTableItemStub : ITableItem
{
    private double _g;

    [ExcelColumn(Name = "A")]
    public string? Name           { get; set; }

    [ExcelColumn(Name = "Article", Aliases = ["B"])]
    public string? Article        { get; set; }

    public int C { get; set; }

    [ExcelIgnore]
    public int D { get; set; }

    public double E { get; set; }

    public double Ff { get; set; }

    public double G
    {
        get => _g;
        set => _g = Math.Clamp(value, 0, double.MaxValue);
    }

    public bool HasValidName()
    {
        return !string.IsNullOrEmpty(Name);
    }

    public bool HasValidArticle()
    {
        return !string.IsNullOrEmpty(Article);
    }
}

public class TableItemStub : ITableItem
{
    public string? Name    { get; set; }
    public string? Article { get; set; }

    public bool HasValidName()
    {
        return !string.IsNullOrEmpty(Name);
    }

    public bool HasValidArticle()
    {
        return !string.IsNullOrEmpty(Article);
    }
}