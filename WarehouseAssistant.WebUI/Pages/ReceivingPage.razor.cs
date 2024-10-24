using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Models;

namespace WarehouseAssistant.WebUI.Pages;

public partial class ReceivingPage : ComponentBase
{
    [Inject] public IRepository<ReceivingItem> Repository        { get; set; } = null!;
    [Inject] public IRepository<Product>       ProductRepository { get; set; } = null!;
    [Inject] public ISnackbar                  Snackbar          { get; set; } = null!;
    [Inject] public IJSRuntime                 JsRuntime         { get; set; } = null!;
    
    private Table<ReceivingItem> _table = null!;
    private bool                 _isBusy;
    
    private List<Product>? _dbProducts;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _dbProducts = await ProductRepository.GetAllAsync();
        }
        catch (HttpRequestException e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Debug.WriteLine(e);
        }
    }
    
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
    
    private void OnInputProvided(ReceivingInputData obj)
    {
        Receive(obj);
    }
    
    private enum NotificationSoundType
    {
        Success,
        Error,
        Warning
    }
    
    private async Task PlaySound(NotificationSoundType soundType)
    {
        string soundUrl = soundType switch
        {
            NotificationSoundType.Success => "_content/WarehouseAssistant.WebUI/sounds/success.mp3",
            NotificationSoundType.Error   => "_content/WarehouseAssistant.WebUI/sounds/error.wav",
            NotificationSoundType.Warning => "_content/WarehouseAssistant.WebUI/sounds/warning.mp3",
            _                             => throw new ArgumentOutOfRangeException(nameof(soundType), soundType, null)
        };
        
        await JsRuntime.InvokeVoidAsync("playNotificationSound", soundUrl);
    }
    
    private void Receive(ReceivingInputData obj)
    {
        ReceivingItem? tableItem = null;
        
        foreach (ReceivingItem receivingItem in _table.Items)
        {
            if (receivingItem.Article == obj.Id)
            {
                tableItem = receivingItem;
                break;
            }
            else if (_dbProducts != null &&
                     _dbProducts.Any(p => p.Barcode == obj.Id && p.Article == receivingItem.Article))
            {
                tableItem = receivingItem;
                break;
            }
        }
        
        if (tableItem == null)
        {
            // TODO Play Error sound
            Snackbar.Add($"Не удалось найти товар с идентификатором {obj.Id}.", Severity.Error);
            PlaySound(NotificationSoundType.Error);
            return;
        }
        
        tableItem.ReceivedQuantity += obj.Quantity;
        
        if (tableItem.ReceivedQuantity < tableItem.ExpectedQuantity)
        {
            // TODO Play Info sound
            Snackbar.Add(
                $"Товар {tableItem.Article} получено {obj.Quantity} шт. Осталось {tableItem.ExpectedQuantity - tableItem.ReceivedQuantity} шт.",
                Severity.Info);
        }
        else if (tableItem.ReceivedQuantity > tableItem.ExpectedQuantity)
        {
            Snackbar.Add($"Товар {tableItem.Article} получено больше, чем ожидалось. Превышение количества.",
                Severity.Warning);
        }
        else
        {
            Snackbar.Add($"Товар {tableItem.Article} получен полностью.", Severity.Success);
        }
    }
}