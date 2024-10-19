using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.WebUI.Components;

public partial class Table<TItem> : ComponentBase
    where TItem : class, ITableItem, new()
{
    private            string?                             _searchString;
    [Parameter] public List<TItem>                         Items                  { get; set; } = [];
    [Parameter] public bool                                ReadOnly               { get; set; }
    [Parameter] public RenderFragment<Table<TItem>>?       ToolBarTemplate        { get; set; }
    [Parameter] public RenderFragment?                     ColumnsTemplate        { get; set; }
    [Parameter] public RenderFragment<CellContext<TItem>>? ChildRowTemplate       { get; set; }
    [Parameter] public RenderFragment?                     PagerTemplate          { get; set; }
    [Parameter] public Func<TItem, bool>?                  QuickFilter            { get; set; }
    [Parameter] public Action<TItem>                       OnCanceledEditingItem  { get; set; }
    [Parameter] public Action<HashSet<TItem>>              OnSelectedItemsChanged { get; set; }
    [Parameter] public string                              RowClass               { get; set; }
    [Parameter] public EventCallback<List<TItem>>          TableImported          { get; set; }
    [Parameter] public Func<TItem, int, string>            RowStyleFunc           { get; set; }
    
    private DataGrid<TItem> DataGridRef { get; set; } = null!;
    
    public bool Loading
    {
        get => DataGridRef.Loading;
        set
        {
            DataGridRef.SetLoading(true);
            StateHasChanged();
        }
    }
    
    internal int            SelectedCount => DataGridRef.SelectedItems.Count;
    internal HashSet<TItem> SelectedItems => DataGridRef.SelectedItems;
    
    private bool SearchFunc(TItem arg)
    {
        return string.IsNullOrEmpty(_searchString) || arg.MatchesSearchString(_searchString);
    }
    
    private void OnTableImported(List<TItem> obj)
    {
        Items = obj;
        TableImported.InvokeAsync(obj);
    }
    
    internal async Task RemoveSelectedItemsAsync()
    {
        if (SelectedCount == 0) return;
        
        var itemsToRemove = new List<TItem>(SelectedItems);
        foreach (TItem item in itemsToRemove)
        {
            await DataGridRef.SetSelectedItemAsync(item);
            Items.Remove(item);
        }
        
        StateHasChanged();
        OnSelectedItemsChanged?.Invoke(SelectedItems);
    }
}