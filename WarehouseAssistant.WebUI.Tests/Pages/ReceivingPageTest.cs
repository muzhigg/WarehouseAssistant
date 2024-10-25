using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Pages;

namespace WarehouseAssistant.WebUI.Tests.Pages;

[TestSubject(typeof(ReceivingPage))]
public class ReceivingPageTest : MudBlazorTestContext
{
    private readonly Mock<IRepository<ReceivingItem>> _receivingRepoMock = new();
    private readonly Mock<IRepository<Product>>       _productRepoMock   = new();
    private readonly Mock<ISnackbar>                  _snackbarMock      = new();
    private readonly Mock<IJSRuntime>                 _jsRuntimeMock     = new();
    
    public ReceivingPageTest()
    {
        Services.AddSingleton(_receivingRepoMock.Object);
        Services.AddSingleton(_productRepoMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }
    
    [Fact]
    public void Receive_Should_AddReceivingItem_When_IdIsArticle()
    {
        // Arrange
        List<Product> products = new()
        {
            new() { Article = "40001234", Name = "Product1" },
            new() { Article = "40002222", Name = "Product2" },
        };
        
        var page = RenderComponent<ReceivingPage>();
    }
}