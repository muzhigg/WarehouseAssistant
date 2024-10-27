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
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.Models;
using Timer = System.Timers.Timer;

namespace WarehouseAssistant.WebUI.Pages;

[UsedImplicitly]
public partial class ReceivingPage : ComponentBase, IDisposable
{
    [Inject] public  IRepository<ReceivingItem> Repository        { get; set; } = null!;
    [Inject] public  IRepository<Product>       ProductRepository { get; set; } = null!;
    [Inject] public  ISnackbar                  Snackbar          { get; set; } = null!;
    [Inject] public  IJSRuntime                 JsRuntime         { get; set; } = null!;
    [Inject] public  IDialogService             DialogService     { get; set; } = null!;
    [Inject] private IProductFormDialogService  ProductFormDialog { get; set; } = null!;
    
    private Table<ReceivingItem> _table = null!;
    private bool                 _isBusy;
    
    private          List<Product>? _dbProducts;
    private readonly Timer          _timer           = new Timer(300000);
    private          MudMessageBox  _articleInputBox = null!;
    private          string?        _articleInput;
    
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
    
    private async Task OnInputProvided(ReceivingInputData obj)
    {
        _isBusy = true;
        await Receive(obj);
        _isBusy = false;
        StateHasChanged();
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
        
        if (tableItem == null && obj.Id.Length > 10)
        {
            bool? confirmed = await _articleInputBox.Show();
            if (confirmed is true && string.IsNullOrEmpty(_articleInput) == false)
            {
                if (_dbProducts != null)
                {
                    if (_dbProducts.Any(product => product.Article == _articleInput))
                    {
                        var dbProduct = _dbProducts.First(product => product.Article == _articleInput);
                        dbProduct.Barcode = obj.Id;
                        try
                        {
                            await ProductRepository.UpdateAsync(dbProduct);
                        }
                        catch (HttpRequestException e)
                        {
                            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
                            Debug.WriteLine(e);
                        }
                        
                        tableItem = _table.Items.FirstOrDefault(item => item.Article == _articleInput);
                    }
                    else
                    {
                        tableItem = _table.Items.FirstOrDefault(item => item.Article == _articleInput);
                        
                        if (tableItem != null)
                        {
                            Product dbProduct = new Product()
                            {
                                Article = _articleInput,
                                Barcode = obj.Id,
                                Name    = tableItem.Name,
                            };
                            var success = await ProductFormDialog.ShowAddDialogAsync(dbProduct);
                            
                            if (success)
                                _dbProducts.Add(dbProduct);
                        }
                    }
                }
            }
        }
        
        if (tableItem == null)
        {
            Snackbar.Add($"Не удалось найти товар с идентификатором {obj.Id}.", Severity.Error);
            return;
        }
        
        tableItem.ReceivedQuantity += obj.Quantity;
        
        if (tableItem.ReceivedQuantity < tableItem.ExpectedQuantity)
        {
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