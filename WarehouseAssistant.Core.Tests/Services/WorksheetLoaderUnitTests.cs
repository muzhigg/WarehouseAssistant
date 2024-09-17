using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using Moq;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests;

[Trait("Category", "Unit")]
public sealed class WorksheetLoaderUnitTests
{
    private sealed class FlagStream : MemoryStream
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return base.DisposeAsync();
        }
    }

    private sealed class TableItemStub : ITableItem
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

    private readonly Mock<IExcelQueryService> _mockExcelQueryService = new();

    [Fact]
    public void Constructor_WithPath_ShouldInitializeStream()
    {
        // Arrange
        string filePath          = "test.xlsx";
        IExcelQueryService excelQueryService = new Mock<IExcelQueryService>().Object;

        // Act
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(filePath, excelQueryService);

        // Assert
        Assert.NotNull(loader);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenStreamIsNotReadable()
    {
        // Arrange
        Mock<Stream> mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.CanRead).Returns(false);

        IExcelQueryService mockExcelQueryService = new Mock<IExcelQueryService>().Object;

        // Act & Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new WorksheetLoader<TableItemStub>(mockStream.Object, mockExcelQueryService)
        );
        Assert.Contains("Stream must be readable.", exception.Message);
        Assert.Equal("stream", exception.ParamName);
    }

    private List<dynamic> GetCorrectData()
    {
        List<dynamic> result = [new Dictionary<string, object> { { "A", "Column1" }, { "B", "Column2" } }];

        return result;
    }

    [Fact]
    public void GetColumns_ShouldReturnCorrectColumns()
    {
        // Arrange
        List<dynamic> data = GetCorrectData();
        _mockExcelQueryService.Setup(s => s.QueryAsync(It.IsAny<Stream>(),
                ExcelType.XLSX))
            .ReturnsAsync(data.AsEnumerable());

        MemoryStream                   stream = new MemoryStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        Dictionary<string, string?> result = loader.GetColumns();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Column1", result["A"]);
        Assert.Equal("Column2", result["B"]);
    }

    [Fact]
    public void GetColumns_ShouldHandleEmptyFile()
    {
        // Arrange
        _mockExcelQueryService.Setup(s => s.QueryAsync(It.IsAny<Stream>(), ExcelType.XLSX)).ReturnsAsync([]);

        MemoryStream                   stream = new MemoryStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        Dictionary<string, string?> result = loader.GetColumns();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetColumnsAsync_ShouldReturnCorrectColumns()
    {
        // Arrange
        List<dynamic> data = GetCorrectData();

        _mockExcelQueryService.Setup(s => s.QueryAsync(It.IsAny<Stream>(), ExcelType.XLSX)).ReturnsAsync(data.AsEnumerable());

        MemoryStream                   stream = new MemoryStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        Dictionary<string, string?> result = await loader.GetColumnsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Column1", result["A"]);
        Assert.Equal("Column2", result["B"]);
    }

    [Fact]
    public async Task GetColumnsAsync_ShouldHandleEmptyFile()
    {
        // Arrange
        _mockExcelQueryService.Setup(s => s.QueryAsync(It.IsAny<Stream>(),
                ExcelType.XLSX))
            .ReturnsAsync([]);

        MemoryStream                   stream = new MemoryStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        Dictionary<string, string?> result = await loader.GetColumnsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ParseItems_ShouldReturnValidItems()
    {
        // Arrange
        TableItemStub validItemStub = new TableItemStub { Name = "ValidName", Article = "ValidArticle" };
        TableItemStub invalidNameItemStub = new TableItemStub { Name = "", Article = "InvalidArticle" };
        TableItemStub invalidArticleItemStub = new TableItemStub { Name = "ValidName", Article = "" };

        List<TableItemStub> data = [validItemStub, invalidNameItemStub, invalidArticleItemStub];
        _mockExcelQueryService.Setup(s => s.Query<TableItemStub>(It.IsAny<Stream>(),
                It.IsAny<ExcelType>(),
                It.IsAny<OpenXmlConfiguration>()))
            .Returns(data.AsEnumerable());

        MemoryStream                   stream = new MemoryStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        IEnumerable<TableItemStub> result = loader.ParseItems();

        // Assert
        IEnumerable<TableItemStub> testTableItems = result.ToList();
        Assert.Single(testTableItems);
        Assert.Contains(validItemStub, testTableItems);
    }

    [Fact]
    public void Dispose_ShouldDisposeStream()
    {
        // Arrange
        FlagStream                     stream = new FlagStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        loader.Dispose();

        // Assert
        Assert.True(stream.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsync_ShouldDisposeStream()
    {
        // Arrange
        FlagStream                     stream = new FlagStream();
        WorksheetLoader<TableItemStub> loader = new WorksheetLoader<TableItemStub>(stream, _mockExcelQueryService.Object);

        // Act
        await loader.DisposeAsync();

        // Assert
        Assert.True(stream.IsDisposed);
    }

    [Fact]
    public void Constructor_ShouldThrowExceptionForInvalidPath()
    {
        // Arrange
        IExcelQueryService excelQueryService = new Mock<IExcelQueryService>().Object;

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => new WorksheetLoader<TableItemStub>("invalid_path.xlsx", excelQueryService));
    }
}