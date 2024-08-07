using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Forms;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.WebUI.Dialogs;

namespace WarehouseAssistant.WebUI.Tests;

public class WorksheetUploadDialogTest : TestContext
{
    public WorksheetUploadDialogTest()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddScoped(_ => new HttpClient());
        Services.AddOptions();
    }

    // Должно открыться и чтобы кнопка была не активна
    [Fact]
    public async Task DialogShouldOpenAndAddButtonShouldBeDisabled()
    {
        // Arrange
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);
        IDialogReference?                     dialogReference = null;

        // Act
        await provider.InvokeAsync(() => dialogReference = service?.Show<WorksheetUploadDialog<ProductTableItemStub>>());
        IElement? submitButton = provider.FindAll("button").FirstOrDefault(element =>
            element.TextContent.Contains("Добавить", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(service); // check service
        Assert.NotNull(dialogReference); // dialog window
        Assert.NotNull(provider.Find("div.mud-dialog-container")); // dialog render
        Assert.NotNull(provider.FindComponent<MudFileUpload<IBrowserFile>>()); // upload button
        Assert.NotNull(submitButton);
        Assert.True(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task DialogShouldOpenWithParamsAndAddButtonShouldBeDisabled()
    {
        // Arrange
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);
        IDialogReference?                     dialogReference = null;

        // Act
        DialogParameters<WorksheetUploadDialog<ProductTableItemStub>> parameters =
            new()
            {
                {
                    dialog => dialog.ExcelColumns, [
                        new DynamicExcelColumn("Name"),
                        new DynamicExcelColumn("Article"),
                        new DynamicExcelColumn("Available")
                    ]
                }
            };
        await provider.InvokeAsync(() => dialogReference = service?.Show<WorksheetUploadDialog<ProductTableItemStub>>("Title", parameters));
        IElement? submitButton = provider.FindAll("button").FirstOrDefault(element =>
            element.TextContent.Contains("Добавить", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(service); // check service
        Assert.NotNull(dialogReference); // dialog window
        Assert.NotNull(provider.Find("div.mud-dialog-container")); // dialog render
        Assert.NotNull(provider.FindComponent<MudFileUpload<IBrowserFile>>()); // upload button
        Assert.NotNull(submitButton);
        Assert.True(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task DialogShouldDisplayColumnSettings()
    {
        // Arrange
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);
        DialogParameters<WorksheetUploadDialog<ProductTableItemStub>> parameters =
            new()
            {
                {
                    dialog => dialog.ExcelColumns, [
                        new DynamicExcelColumn("Name"),
                        new DynamicExcelColumn("Article"),
                        new DynamicExcelColumn("Available")
                    ]
                }
            };
        await provider.InvokeAsync(() => service?.Show<WorksheetUploadDialog<ProductTableItemStub>>("Title", parameters));
        List<IDictionary<string, object>> data =
        [
            new Dictionary<string, object>
            {
                { "Номенклатура", "Test Name 1" },
                { "Артикул", "4001" },
                { "Доступно основной склад МО", 1 },
            }
        ];
        using MemoryStream fakeStream = CreateFakeExcelStream(data, true);

        InputFileContent              inputFileContent  = InputFileContent.CreateFromBinary(fakeStream.ToArray(), "Test WOrk.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        IRenderedComponent<InputFile> input = provider.FindComponent<InputFile>();

        // Act
        input.UploadFiles(inputFileContent);
        await Task.Delay(1000);

        // Assert
        IReadOnlyList<IRenderedComponent<MudSelect<string>>> selectFields = provider.FindComponents<MudSelect<string>>();
        Assert.Equal(3, selectFields.Count);

        Assert.Equal("A", selectFields[0].Instance.Value);
        Assert.Equal("A", provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Name"]);

        Assert.Equal("B", selectFields[1].Instance.Value);
        Assert.Equal("B", provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Article"]);

        Assert.Equal("C", selectFields[2].Instance.Value);
        Assert.Equal("C", provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Available"]);

        await provider.InvokeAsync(async () => await selectFields[0].Instance.SelectOption("B"));

        Assert.Equal("B", selectFields[0].Instance.Value);
        Assert.Equal("B", provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Name"]);

        IElement? submitButton = provider.FindAll("button").FirstOrDefault(element =>
            element.TextContent.Contains("Добавить", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(submitButton);
        Assert.False(submitButton.HasAttribute("disabled"));
    }

    private MemoryStream CreateFakeExcelStream(object rows, bool printHeader)
    {
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.SaveAs(rows, printHeader);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }

    private IRenderedComponent<MudDialogProvider> RenderedDialogProvider(out DialogService? service)
    {
        IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
        service = Services.GetService<IDialogService>() as DialogService;
        return provider;
    }

    [Fact]
    public async Task ShouldNotToDisplayColumnSettings()
    {
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);

        await provider.InvokeAsync(() => service?.Show<WorksheetUploadDialog<ProductTableItemStub>>("Title"));
        List<IDictionary<string, object>> data =
        [
            new Dictionary<string, object>
            {
                { "Номенклатура", "Test Name 1" },
                { "Артикул", "4001" },
                { "Доступно основной склад МО", 1 },
            }
        ];

        using MemoryStream fakeStream = CreateFakeExcelStream(data, true);

        InputFileContent inputFileContent = InputFileContent.CreateFromBinary(fakeStream.ToArray(), "Test WOrk.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        IRenderedComponent<InputFile> input = provider.FindComponent<InputFile>();

        input.UploadFiles(inputFileContent);
        await Task.Delay(1000);

        IReadOnlyList<IRenderedComponent<MudSelect<string>>> selectFields = provider.FindComponents<MudSelect<string>>();
        Assert.Empty(selectFields);

        IElement? submitButton = provider.FindAll("button").FirstOrDefault(element =>
            element.TextContent.Contains("Добавить", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(submitButton);
        Assert.False(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task ShouldDisplayEmptySelectFields()
    {
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);

        DialogParameters<WorksheetUploadDialog<ProductTableItemStub>> parameters =
            new()
            {
                {
                    dialog => dialog.ExcelColumns, [
                        new DynamicExcelColumn("Name"),
                        new DynamicExcelColumn("Article"),
                        new DynamicExcelColumn("Available")
                    ]
                }
            };
        await provider.InvokeAsync(() => service?.Show<WorksheetUploadDialog<ProductTableItemStub>>("Title", parameters));
        List<IDictionary<string, object>> data = [];

        using MemoryStream fakeStream = CreateFakeExcelStream(data, true);

        InputFileContent inputFileContent = InputFileContent.CreateFromBinary(fakeStream.ToArray(), "Test WOrk 2.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        IRenderedComponent<InputFile> input = provider.FindComponent<InputFile>();

        input.UploadFiles(inputFileContent);
        await Task.Delay(1000);

        IReadOnlyList<IRenderedComponent<MudSelect<string>>> selectFields = provider.FindComponents<MudSelect<string>>();
        Assert.Equal(3, selectFields.Count);

        IElement? submitButton = provider.FindAll("button").FirstOrDefault(element =>
            element.TextContent.Contains("Добавить", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(submitButton);
        Assert.True(submitButton.HasAttribute("disabled"));

        Assert.Null(selectFields[0].Instance.Value);
        Assert.Null(provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Name"]);
        Assert.Null(selectFields[1].Instance.Value);
        Assert.Null(provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Article"]);
        Assert.Null(selectFields[2].Instance.Value);
        Assert.Null(provider.FindComponent<WorksheetUploadDialog<ProductTableItemStub>>().Instance.RequiredColumns["Available"]);
    }
}

public class ProductTableItemStub : ICalculatedTableItem
{
    [ExcelColumn(Name = "Название", Aliases = ["Номенклатура"])]
    public string? Name { get; set; }

    [ExcelColumn(Name = "Артикул")]
    public string? Article { get; set; }

    [ExcelColumn(Name = "Доступно на БГЛЦ", Aliases = ["Доступно основной склад МО"])]
    public int Available { get; set; }

    public bool HasValidName()
    {
        return !string.IsNullOrEmpty(Name);
    }

    public bool HasValidArticle()
    {
        return !string.IsNullOrEmpty(Article);
    }

    public int QuantityToOrder { get; set; }
    public int MaxCanBeOrdered => (int)(Available * 0.07);
}