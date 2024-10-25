using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Components;

public partial class Notification : ComponentBase
{
    private MudSnackbarElement _snackbar;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Debug.WriteLine(_snackbar.Snackbar.Severity);
    }
}