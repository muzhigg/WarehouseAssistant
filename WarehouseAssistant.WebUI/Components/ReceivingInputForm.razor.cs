using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using WarehouseAssistant.WebUI.Models;

namespace WarehouseAssistant.WebUI.Components;

public partial class ReceivingInputForm : MudComponentBase
{
    [Parameter] public bool                              Disabled      { get; set; }
    [Parameter] public EventCallback<ReceivingInputData> OnInputSubmit { get; set; }
    private            bool                              _hidden;
    private            ReceivingInputData                _model = new();
    
    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue("hidden", out bool hidden)) _hidden = hidden;
        
        return base.SetParametersAsync(parameters);
    }
    
    private async Task OnValidSubmit(EditContext obj)
    {
        if (_model.Id.Length == 4)
            _model.Id = "4000" + _model.Id;
        
        await OnInputSubmit.InvokeAsync(_model);
        _model = new ReceivingInputData();
    }
}