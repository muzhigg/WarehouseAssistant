using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.Core.Tests.Services;

[Trait("Category", "Integration")]
public sealed class WorkbookBuilderIntegrationTests
{
    private sealed class TableITemStub : ITableItem
    {
        public string? Name           { get; set; }
        public string? Article        { get; set; }
        
        public bool    HasValidName()
        {
            return string.IsNullOrEmpty(Name) == false;
        }
        
        public bool    HasValidArticle()
        {
            return string.IsNullOrEmpty(Article) == false;
        }
    }
    
    [Fact]
    public void BuildAndParseWorkbook()
    {
        // Arrange
        var workbookBuilder = new WorkbookBuilder<TableITemStub>();
        var tableItems = new List<TableITemStub>
        {
            new TableITemStub { Name = "Item 1", Article = "Article 1" },
            new TableITemStub { Name = "Item 2", Article = "Article 2" },
            new TableITemStub { Name = "Item 3", Article = "Article 3" },
        };
        workbookBuilder.CreateSheet("Sheet 1");
        workbookBuilder.AddRangeToSheet("Sheet 1", tableItems);
        MemoryStream stream = new MemoryStream(workbookBuilder.AsByteArray());
        stream.Position = 0;
        var          loader = new WorksheetLoader<TableITemStub>(stream);
        
        // Act
        IEnumerable<TableITemStub> parsedItems = loader.ParseItems().ToList();
        
        // Assert
        Assert.Equivalent(tableItems, parsedItems);
    }
}