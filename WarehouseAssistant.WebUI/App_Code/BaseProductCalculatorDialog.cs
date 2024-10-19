using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.WebUI.ProductOrder;

public class BaseProductCalculatorDialog : ComponentBase
{
    [Inject]    public ISnackbar                      Snackbar          { get; set; } = null!;
    [Inject]    public IDialogService                 DialogService     { get; set; } = null!;
    [Inject]    public ILocalStorageService           LocalStorage      { get; set; } = null!;
    [Parameter] public IEnumerable<ProductTableItem>? ProductTableItems { get; set; }
}