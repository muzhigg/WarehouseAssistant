using Blazored.LocalStorage;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Tests.Dialogs;

public class ProductCalculatorDialogTest : MudBlazorTestContext
{
    private Mock<IRepository<Product>> _repositoryMock;
    private Mock<IDialogService>       _dialogServiceMock;
    private Mock<ISnackbar>            _snackbarMock;
    private Mock<ILocalStorageService> _localStorageMock;
    
    public ProductCalculatorDialogTest()
    {
        _repositoryMock    = new Mock<IRepository<Product>>();
        _dialogServiceMock = new Mock<IDialogService>();
        _snackbarMock      = new Mock<ISnackbar>();
        _localStorageMock  = new Mock<ILocalStorageService>();
        Services.AddSingleton(_repositoryMock.Object);
        Services.AddSingleton(_dialogServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddSingleton(_localStorageMock.Object);
        Services.AddMudEventManager().AddMudLocalization().AddMudBlazorKeyInterceptor()
            .AddMudBlazorScrollManager().AddMudPopoverService();
    }
}