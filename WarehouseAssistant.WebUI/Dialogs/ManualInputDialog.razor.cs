using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

[assembly: InternalsVisibleTo("WarehouseAssistant.WebUI.Tests")]

namespace WarehouseAssistant.WebUI.Dialogs;

public sealed partial class ManualInputDialog<T> where T : IMinMaxValue<T>, IEquatable<T>
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter] public string Text { get; set; } = "Введите число";
    
    [Parameter] public T Min { get; set; } = T.MinValue;
    [Parameter] public T Max { get; set; } = T.MaxValue;
    
    [Parameter] public T Value { get; set; } = default!;
    
    private T _initialValue = default!;
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _initialValue = Value;
    }
    
    private void ManualInputSubmit(MouseEventArgs obj)
    {
        if (!_initialValue.Equals(Value))
            MudDialog.Close(DialogResult.Ok(Value));
        else
            MudDialog.Cancel();
    }
    
    private void ManualInputCancel(MouseEventArgs obj)
    {
        MudDialog.Cancel();
    }
}