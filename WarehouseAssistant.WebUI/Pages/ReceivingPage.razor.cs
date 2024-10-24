using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.WebUI.Components;

namespace WarehouseAssistant.WebUI.Pages;

public partial class ReceivingPage : ComponentBase
{
    [Inject] public ReceivingItemRepository Repository { get; set; }
    [Inject] public ISnackbar               Snackbar   { get; set; }
    
    private Table<ReceivingItem> _table = null!;
    private bool                 _isBusy;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // _isBusy = true;
        //
        // try
        // {
        //     Debug.WriteLine("ReceivingPage: Trying to load items.");
        //     
        //     if (await Repository.HasActiveSessionAsync())
        //     {
        //         Debug.WriteLine("ReceivingPage: Active session found.");
        //         var items = await Repository.GetItemsAsync();
        //         _table.Items = items;
        //         StateHasChanged();
        //     }
        // }
        // catch (Exception e)
        // {
        //     Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
        // }
        // finally
        // {
        //     _isBusy = false;
        // }
        
        await base.OnAfterRenderAsync(firstRender);
    }
}