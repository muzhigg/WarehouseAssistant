using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WarehouseAssistant.WebUI.Models;

namespace WarehouseAssistant.WebUI.Components;

public partial class ReceivingInputForm : MudComponentBase
{
    [Parameter] public bool               Disabled { get; set; }
    private            bool               _hidden;
    private            string             _idInput       = "";
    private            int                _quantityInput = 1;
    private            ReceivingInputData _model         = new();
    
    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue("hidden", out bool hidden)) _hidden = hidden;
        
        return base.SetParametersAsync(parameters);
    }
    
    private void OnValidSubmit(EditContext obj)
    {
        Debug.WriteLine("Valid submit");
    }
}