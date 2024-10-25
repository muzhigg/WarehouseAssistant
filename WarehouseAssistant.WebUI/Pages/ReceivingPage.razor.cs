using System.Diagnostics;
using System.Timers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.Models;
using Timer = System.Timers.Timer;

namespace WarehouseAssistant.WebUI.Pages;

[UsedImplicitly]
public partial class ReceivingPage : ComponentBase, IDisposable
{
    [Inject] public IRepository<ReceivingItem> Repository        { get; set; } = null!;
    [Inject] public IRepository<Product>       ProductRepository { get; set; } = null!;
    [Inject] public ISnackbar                  Snackbar          { get; set; } = null!;
    [Inject] public IJSRuntime                 JsRuntime         { get; set; } = null!;
    [Inject] public IDialogService             DialogService     { get; set; } = null!;
    
    private Table<ReceivingItem> _table = null!;
    private bool                 _isBusy;
    
    private List<Product>? _dbProducts;
    private Timer          _timer = new Timer(300000);
    
    protected override async Task OnInitializedAsync()
    {
        _timer.Elapsed   += TimerOnElapsed;
        _timer.AutoReset =  true;
        
        
        try
        {
            _isBusy     = true;
            _dbProducts = await ProductRepository.GetAllAsync();
            Debug.WriteLine("ReceivingPage: Trying to load last session.");
            
            var items = await Repository.GetAllAsync();
            
            if (items != null && items.Count != 0)
            {
                Debug.WriteLine("ReceivingPage: Active session found.");
                _table.Items   = items;
                _timer.Enabled = true;
                StateHasChanged();
            }
        }
        catch (HttpRequestException e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Debug.WriteLine(e);
        }
        finally
        {
            _isBusy = false;
        }
    }
    
    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        Repository.UpdateRangeAsync(_table.Items);
        Snackbar.Add("Данные сохранены.", Severity.Success);
    }
    
    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender)
    //     {
    //         
    //         
    //         try
    //         {
    //             
    //         }
    //         catch (Exception e)
    //         {
    //             Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
    //         }
    //         finally
    //         {
    //             
    //         }
    //     }
    //     
    //     await base.OnAfterRenderAsync(firstRender);
    // }
    
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
    
    internal async Task Receive(ReceivingInputData obj)
    {
        ReceivingItem? tableItem = null;
        
        foreach (ReceivingItem receivingItem in _table.Items)
        {
            if (receivingItem.Article == obj.Id)
            {
                tableItem = receivingItem;
                break;
            }
            
            if (_dbProducts != null &&
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
    
    private async Task CompleteReceiving()
    {
        // TODO: Complete receiving session and save data to database.
        // TODO: Play Success sound
        
        bool? confirmed = await DialogService.ShowMessageBox("Вы уверены?",
            "Приёмка будет завершена. Все накопленные данные будут удалены. Продолжить?", yesText: "Да", noText: "Нет");
        
        if (confirmed == false)
            return;
        
        Snackbar.Add("Приёмка завершена.", Severity.Success);
        
        try
        {
            _isBusy = true;
            await Repository.DeleteRangeAsync(_table.Items.Select(item => item.Article));
            Snackbar.Add("Данные удалены.", Severity.Success);
        }
        catch (Exception e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Debug.WriteLine(e);
        }
        finally
        {
            _isBusy        = false;
            _timer.Enabled = false;
        }
        
        _table.Items.Clear();
    }
    
    private async Task OnTableImported(List<ReceivingItem> obj)
    {
        _isBusy        = true;
        _timer.Enabled = true;
        
        try
        {
            await Repository.AddRangeAsync(obj);
            Snackbar.Add("Данные сохранены.", Severity.Success);
        }
        catch (Exception e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Debug.WriteLine(e);
        }
        finally
        {
            _isBusy = false;
        }
    }
    
    public void Dispose()
    {
        _timer.Dispose();
    }
}