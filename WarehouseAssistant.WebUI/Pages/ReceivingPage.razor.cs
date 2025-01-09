using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using WarehouseAssistant.WebUI.Components;
using WarehouseAssistant.WebUI.DatabaseModule;
using WarehouseAssistant.WebUI.Models;

namespace WarehouseAssistant.WebUI.Pages;

[UsedImplicitly]
public partial class ReceivingPage : ComponentBase
{
    [Inject] private IRepository<ReceivingItem> Repository        { get; set; } = null!;
    [Inject] private IRepository<Product>       ProductRepository { get; set; } = null!;
    [Inject] private ISnackbar                  Snackbar          { get; set; } = null!;
    [Inject] private IJSRuntime                 JsRuntime         { get; set; } = null!;
    [Inject] private IDialogService             DialogService     { get; set; } = null!;
    [Inject] private IProductFormDialogService  ProductFormDialog { get; set; } = null!;
    [Inject] private ILogger<ReceivingPage>     Logger            { get; set; } = null!;
    
    private Table<ReceivingItem> _table = null!;
    private bool                 _isBusy;
    
    private List<Product>? _dbProducts;
    private MudMessageBox  _articleInputBox = null!;
    private string?        _articleInput;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _isBusy     = true;
            _dbProducts = await ProductRepository.GetAllAsync();
            
            var items = await Repository.GetAllAsync();
            
            if (items?.Count > 0) _table.Items = items;
        }
        catch (HttpRequestException e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Logger.LogError(e, e.Message);
        }
        finally
        {
            _isBusy = false;
        }
    }
    
    private async Task OnInputProvided(ReceivingInputData obj)
    {
        _isBusy = true;
        await Receive(obj);
        _isBusy = false;
    }
    
    private async Task<ReceivingItem?> FindByBarcodeInDatabaseAsync(ReceivingInputData obj)
    {
        ReceivingItem? tableItem = null;
        
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
                    Logger.LogError(e, e.Message);
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
        
        return tableItem;
    }
    
    private async Task Receive(ReceivingInputData obj)
    {
        ReceivingItem? tableItem = null;
        
        tableItem = FindTableItem(obj);
        
        if (tableItem == null && obj.Id.Length > 10)
        {
            bool? confirmed = await _articleInputBox.Show();
            if (confirmed is true && string.IsNullOrEmpty(_articleInput) == false)
            {
                await FindByBarcodeInDatabaseAsync(obj);
                _articleInput = null;
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
            ShowToastWithTooltip(
                $"Товар {tableItem.Article} получено {obj.Quantity} шт. Осталось {tableItem.ExpectedQuantity - tableItem.ReceivedQuantity} шт.",
                tableItem.Name,
                Severity.Info);
        }
        else if (tableItem.ReceivedQuantity > tableItem.ExpectedQuantity)
        {
            ShowToastWithTooltip(
                $"Товар {tableItem.Article} получено больше, чем ожидалось. Превышение количества.",
                tableItem.Name,
                Severity.Warning);
        }
        else
        {
            ShowToastWithTooltip(
                $"Товар {tableItem.Article} получен полностью.",
                tableItem.Name,
                Severity.Success);
        }
    }
    
    private ReceivingItem? FindTableItem(ReceivingInputData obj)
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
        
        return tableItem;
    }
    
    private async Task CompleteReceiving()
    {
        bool? confirmed = await DialogService.ShowMessageBox("Вы уверены?",
            "Приёмка будет завершена. Все накопленные данные будут удалены. Продолжить?", yesText: "Да", noText: "Нет");
        
        if (confirmed == false)
            return;
        
        try
        {
            _isBusy = true;
            await Repository.DeleteRangeAsync(_table.Items);
            _table.Items.Clear();
            Snackbar.Add("Приёмка завершена и данные удалены.", Severity.Success);
        }
        catch (Exception e)
        {
            Snackbar.Add($"Ошибка при обращении к базе данных: {e.Message}", Severity.Error);
            Logger.LogError(e, e.Message);
        }
        finally
        {
            _isBusy = false;
        }
    }
    
    private async Task SaveTableState()
    {
        try
        {
            await Repository.UpdateRangeAsync(_table.Items);
            Snackbar.Add("Данные сохранены.", Severity.Info);
        }
        catch (Exception e)
        {
            Snackbar.Add($"Ошибка при сохранении состояния таблицы: {e.Message}", Severity.Error);
            Logger.LogError(e, e.Message);
        }
    }
}