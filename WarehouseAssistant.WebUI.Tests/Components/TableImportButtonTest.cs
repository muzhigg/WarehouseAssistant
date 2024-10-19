using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.WebUI.Components;

namespace WarehouseAssistant.WebUI.Tests.Components;

public class TableImportButtonTest : MudBlazorTestContext
{
    [Fact]
    public void OnInitialized_ShouldRenderMudSelectForEachValidProperty()
    {
        // Arrange
        Services.AddMudServices();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        
        // Act
        IRenderedComponent<TableImportButton<TestTableItem>> component =
            RenderComponent<TableImportButton<TestTableItem>>();
        component.Find(".table-import-button").Click();
        
        // Assert
        var mudSelects = dialogProvider.FindAll(".import-column-select");
        mudSelects.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task OnFilesChanged_ShouldPopulateColumns_WhenFileIsSelected()
    {
        // Arrange
        Services.AddMudServices();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        IRenderedComponent<TableImportButton<TestTableItem>> component =
            RenderComponent<TableImportButton<TestTableItem>>();
        component.Find(".table-import-button").Click();
        
        var values = new List<Dictionary<string, object>>()
        {
            new() { { "Name", "MiniExcel" }, { "Article", "1" } },
            new() { { "Name", "Github" }, { "Article", "2" } }
        };
        
        using var stream = new MemoryStream();
        await stream.SaveAsAsync(values);
        stream.Position = 0;
        
        // Act
        dialogProvider.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(stream.ToArray(),
            "test.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        
        // Assert
        dialogProvider.FindComponent<MudFileUpload<IBrowserFile>>().Instance.Files.Should().NotBeNull();
        dialogProvider.FindComponent<MudSelect<string>>().FindComponents<MudSelectItem<string>>().Should().HaveCount(2);
        dialogProvider.FindComponents<MudSelect<string>>().Should()
            .Contain(renderedComponent => renderedComponent.Instance.Value == "A");
        dialogProvider.FindComponents<MudSelect<string>>().Should()
            .Contain(renderedComponent => renderedComponent.Instance.Value == "B");
    }
    
    [Fact]
    public async Task ImportFile_ShouldParseItems_WhenColumnsAreMappedCorrectly()
    {
        // Arrange
        List<TestTableItem> parsedItems = new();
        Services.AddMudServices();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        IRenderedComponent<TableImportButton<TestTableItem>> component =
            RenderComponent<TableImportButton<TestTableItem>>(builder =>
                builder.Add(b => b.OnParsed, items => parsedItems = items));
        component.Find(".table-import-button").Click();
        
        var values = new List<Dictionary<string, object>>()
        {
            new() { { "Name", "MiniExcel" }, { "Article", "1" }, { "Description", "A description" } },
            new() { { "Name", "Github" }, { "Article", "2" }, { "Description", "Another description" } }
        };
        
        using var stream = new MemoryStream();
        await stream.SaveAsAsync(values);
        stream.Position = 0;
        
        dialogProvider.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromBinary(stream.ToArray(),
            "test.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        
        // Act
        var importButton = dialogProvider.Find("button:contains('Импортировать')");
        importButton.Click();
        
        // Assert
        parsedItems.Should().NotBeNull().And.HaveCount(2);
    }
    
    public class TestTableItem : ITableItem
    {
        [ExcelColumn(Name = "Name")]    public string Name    { get; set; } = string.Empty;
        [ExcelColumn(Name = "Article")] public string Article { get; set; } = string.Empty;
        
        public string Id { get; set; } = string.Empty;
        
        [ExcelColumn(Name = "Description", Ignore = true)]
        public string Description { get; set; } = string.Empty;
        
        public bool HasValidName()    => !string.IsNullOrEmpty(Name);
        public bool HasValidArticle() => !string.IsNullOrEmpty(Article);
        
        public bool MatchesSearchString(string searchString)
        {
            return true;
        }
    }
}