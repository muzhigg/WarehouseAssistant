using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Shared.Models;

namespace WarehouseAssistant.WebUI.Components;

public partial class Table<TItem> : ComponentBase
    where TItem : class, ITableItem, new()
{
    private            string?                             _searchString;
    [Parameter] public List<TItem>?                        Items                  { get; set; } = new();
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
    
    public int            SelectedCount => DataGridRef.SelectedItems.Count;
    public HashSet<TItem> SelectedItems => DataGridRef.SelectedItems;
    
    private bool SearchFunc(TItem arg)
    {
        return string.IsNullOrEmpty(_searchString) || arg.MatchesSearchString(_searchString);
    }
    
    private void OnTableImported(List<TItem> obj)
    {
        Items = obj;
        TableImported.InvokeAsync(obj);
    }
}