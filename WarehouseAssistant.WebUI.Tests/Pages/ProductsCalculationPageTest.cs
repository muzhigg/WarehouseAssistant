using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.ProductOrder;
using Xunit.Abstractions;

namespace WarehouseAssistant.WebUI.Tests.Pages;

[TestSubject(typeof(DaysBasedCalculatorDialog))]
public class ProductsCalculationPageTest : MudBlazorTestContext
{
    private readonly ITestOutputHelper _logger;
    
    private readonly Mock<ISnackbar> _snackbar = new();
    
    // private readonly Mock<IDialogService> _dialogService = new();
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly Mock<IPopoverService>      _popover           = new();
    
    public ProductsCalculationPageTest(ITestOutputHelper logger)
    {
        _logger = logger;
        Services.AddSingleton(_snackbar.Object);
        // Services.AddSingleton(_dialogService.Object);
        Services.AddSingleton(_productRepository.Object);
        Services.AddMudEventManager();
        Services.AddMudLocalization();
        Services.AddMudBlazorKeyInterceptor().AddMudBlazorScrollManager();
        // Services.AddSingleton(_popover.Object);
    }
    
    [Theory]
    [ClassData(typeof(CalculatorDialogTestData))]
    public async Task OpenCalculationDialog_Should_OpenDialog<TDialog>(
        DialogParameters<TDialog> parameters)
        where TDialog : BaseProductCalculatorDialog
    {
        // Arrange
        Services.AddMudBlazorDialog();
        Services.AddMudPopoverService();
        var dialogProvider = RenderComponent<MudDialogProvider>();
        RenderComponent<MudPopoverProvider>();
        
        var page = RenderComponent<ProductsCalculationPage>();
        
        // Act
        _ = page.Instance.OpenCalculationDialog<DaysBasedCalculatorDialog>();
        await Task.Delay(1000);
        // page.FindComponent<MudMenu>().Instance.OpenMenu();
        
        // Assert
        // _logger.WriteLine(page.Markup);
        _logger.WriteLine(dialogProvider.Markup);
        // dialogProvider.FindComponent<MudDialog>().Should().NotBeNull();
    }
    
    private class CalculatorDialogTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new DialogParameters<DaysBasedCalculatorDialog>() };
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}