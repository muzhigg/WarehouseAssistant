using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using WarehouseAssistant.Shared.Models;

[assembly: InternalsVisibleTo("WarehouseAssistant.WebUI.Tests")]

namespace WarehouseAssistant.WebUI.Dialogs;

public sealed partial class ManualOrderInputDialog<TCalculatedItem> where TCalculatedItem : class, ICalculatedTableItem
{
    public static IDialogReference Show(IDialogService dialogService, TCalculatedItem item, string text,
        RenderFragment?                                additionalRenderFragment = null)
    {
        DialogParameters<ManualOrderInputDialog<TCalculatedItem>> parameters = new()
        {
            { dialog => dialog.Item, item },
            { dialog => dialog.Text, text },
            { dialog => dialog.AdditionalRenderFragment, additionalRenderFragment }
        };
        
        return dialogService.Show<ManualOrderInputDialog<TCalculatedItem>>("Введите количество", parameters);
    }
    
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter, EditorRequired] public TCalculatedItem Item { get; set; } = null!;
    
    [Parameter, EditorRequired] public string Text { get; set; } = null!;
    
    [Parameter] public RenderFragment? AdditionalRenderFragment { get; set; }
    
    internal int Value
    {
        get => Item.QuantityToOrder;
        set => Item.QuantityToOrder = value;
    }
    
    internal int MaxValue => Item.MaxCanBeOrdered;
    
    private int? _initialValue;
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Item == null) throw new ArgumentNullException(nameof(Item), $"{nameof(Item)} is not set as parameter");
        
        if (Text == null) throw new ArgumentNullException(nameof(Text), $"{nameof(Text)} is not set as parameter");
        
        _initialValue ??= Item.QuantityToOrder;
    }
    
    private void ManualInputSubmit(MouseEventArgs obj)
    {
        MudDialog.Close(DialogResult.Ok(Value));
    }
    
    private void ManualInputCancel(MouseEventArgs obj)
    {
        if (_initialValue != Item.QuantityToOrder)
            Item.QuantityToOrder = _initialValue!.Value;
        
        MudDialog.Cancel();
    }
}