using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Components;

public partial class TextWithTooltip : ComponentBase
{
    private MudSnackbarElement _snackbar;
    
    [Parameter] public string Text    { get; set; }
    [Parameter] public string Tooltip { get; set; }
}