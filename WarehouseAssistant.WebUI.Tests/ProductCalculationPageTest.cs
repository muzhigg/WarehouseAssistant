using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Blazored.LocalStorage;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using MiniExcelLibs;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.WebUI.Pages;

namespace WarehouseAssistant.WebUI.Tests;

public class LocalStorageStub : ILocalStorageService
{
    public async ValueTask                        ClearAsync(CancellationToken         cancellationToken                          = new CancellationToken())
    {
        return;
    }

    public async ValueTask<T?>                    GetItemAsync<T>(string               key,   CancellationToken cancellationToken = new CancellationToken())
    {
        return default;
    }

    public async ValueTask<string?>               GetItemAsStringAsync(string          key,   CancellationToken cancellationToken = new CancellationToken())
    {
        return null;
    }

    public async ValueTask<string?>               KeyAsync(int                         index, CancellationToken cancellationToken = new CancellationToken())
    {
        return null;
    }

    public async ValueTask<IEnumerable<string>>   KeysAsync(CancellationToken          cancellationToken                        = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<bool>                  ContainKeyAsync(string               key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<int>                   LengthAsync(CancellationToken        cancellationToken                                                 = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask                        RemoveItemAsync(string               key,  CancellationToken cancellationToken                         = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask                        RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken                         = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask                        SetItemAsync<T>(string      key, T      data, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask                        SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ChangingEventArgs>? Changing;
    public event EventHandler<ChangedEventArgs>?  Changed;
}

public class ProductRepositoryStub : IRepository<Product>
{
    public async Task<Product?>              GetByArticleAsync(string article)
    {
        return null;
    }

    public async Task<IEnumerable<Product>?> GetAllAsync()
    {
        return null;
    }

    public async Task AddAsync(Product    product)
    {
        return;
    }

    public async Task UpdateAsync(Product product)
    {
        return;
    }

    public async Task DeleteAsync(string? article)
    {
        return;
    }
}

public class ProductCalculationPageTest : TestContext
{
    public ProductCalculationPageTest()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
        Services.AddScoped(_ => new HttpClient());
        Services.AddOptions();
        Services.AddScoped<IRepository<Product>, ProductRepositoryStub>();
        Services.AddScoped<ILocalStorageService, LocalStorageStub>();
    }

    private IRenderedComponent<MudDialogProvider> RenderedDialogProvider(out DialogService? service)
    {
        IRenderedComponent<MudDialogProvider> provider = RenderComponent<MudDialogProvider>();
        service = Services.GetService<IDialogService>() as DialogService;
        return provider;
    }

    private IElement FindOpenUploadTableDialogButton(IRenderedFragment fragment)
    {
        var element = fragment.Find(".open-upload-table-dialog-button");
        Assert.NotNull(element);
        return element;
    }

    private IElement FindOpenCalculationDialogButton(IRenderedFragment fragment)
    {
        var element = fragment.Find(".open-calculation-dialog-button");
        Assert.NotNull(element);
        return element;
    }

    private IElement FindSubmitUploadTableButton(IRenderedFragment fragment)
    {
        var element = fragment.Find(".submit-upload-button");
        Assert.NotNull(element);
        return element;
    }

    [Fact]
    public async Task ShouldDisplayUiCorrectly()
    {
        IRenderedComponent<MudDialogProvider> provider        = RenderedDialogProvider(out DialogService? service);

        //open page
        var page = RenderComponent<ProductsCalculationPage>();

        //find upload button
        IElement? uploadButton = FindOpenUploadTableDialogButton(page);
        Assert.False(uploadButton.HasAttribute("hidden"));

        //find calculate button
        var calcButton = FindOpenCalculationDialogButton(page);
        Assert.True(calcButton.HasAttribute("hidden"));

        //click upload button
        uploadButton.Click();
        var dialogInstance = provider.FindComponent<MudDialogInstance>();
        Assert.NotNull(dialogInstance);
        Assert.True(dialogInstance.Instance.Title.Contains("Загрузка файла", StringComparison.OrdinalIgnoreCase));

        //upload table
        var submitButton = FindSubmitUploadTableButton(dialogInstance);
        Assert.True(submitButton.HasAttribute("disabled"));
        List<IDictionary<string, object>> data =
        [
            new Dictionary<string, object>
            {
                { "Номенклатура", "BTSES Anti-wrinkle moisturizing cream – Крем увлажняющий против морщин, 50 мл" },
                { "Артикул", "40000252" },
                { "Доступно основной склад МО", 1585 },
                { "Доступно Санкт-Петербург (склад)", 3 },
                { "Средняя оборачиваемость в день", 0.07 },
                { "Запас товара (на кол-во дней)", 42.86 },
                { "Расчет заказа", -0.64 },
                { "Заказ на офис Спб", 0 },
            }
        ];
        using MemoryStream fakeStream = CreateFakeExcelStream(data, true);

        InputFileContent inputFileContent = InputFileContent.CreateFromBinary(fakeStream.ToArray(), "Test WOrk.xlsx", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        IRenderedComponent<InputFile> input = dialogInstance.FindComponent<InputFile>();
        input.UploadFiles(inputFileContent);
        await Task.Delay(1000);
        submitButton = FindSubmitUploadTableButton(dialogInstance);
        Assert.False(submitButton.HasAttribute("disabled"));
        submitButton.Click();
        await Task.Delay(1000);
        uploadButton = FindOpenUploadTableDialogButton(page);
        Assert.True(uploadButton.HasAttribute("hidden"));
        calcButton = FindOpenCalculationDialogButton(page);
        Assert.False(calcButton.HasAttribute("hidden"));
    }

    private MemoryStream CreateFakeExcelStream(object rows, bool printHeader)
    {
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.SaveAs(rows, printHeader);
        memoryStream.Position = 0; // Reset stream position for reading
        return memoryStream;
    }
}