using MudBlazor;

namespace WarehouseAssistant.WebUI.Components;

public class DataGrid<T> : MudDataGrid<T>
{
    // [Inject] private TableOperationState OperationState { get; set; } = null!;
    
    public new virtual HashSet<T> SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }
    
    internal void SetLoading(bool loading)
    {
        Loading = loading;
    }
}