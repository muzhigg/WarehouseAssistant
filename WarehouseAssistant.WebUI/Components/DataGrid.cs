using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Components;

[CascadingTypeParameter(nameof(T))]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class DataGrid<T> : MudDataGrid<T>
{
    private sealed class FilterDataWrapper
    {
        public int     ColumnIndex { get; init; }
        public Guid    Guid        { get; init; }
        public string? Title       { get; init; }
        public string? Operator    { get; init; }
        public object? Value       { get; set; }
    }
    
    [Parameter] public string LocalStorageKey { get; set; } = "DataGrid";
    
    public new virtual HashSet<T> SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }
    
    internal void SetLoading(bool loading)
    {
        Loading = loading;
    }
    //[Inject] private ILocalStorageService LocalStorage { get; set; } = null!;

    //private          List<FilterDataWrapper> _filterDataWrappers = [];
    //private readonly object                   _filterLock         = new();

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    await base.OnAfterRenderAsync(firstRender);

    //    if (firstRender)
    //    {
    //        await LoadFilters();
    //    }
    //    else
    //    {
    //        if (IsFiltersChanged()) await SaveFilters();
    //    }
    //}

    //private bool IsFiltersChanged()
    //{
    //    lock (_filterLock)
    //    {
    //        if (_filterDataWrappers.Count != FilterDefinitions.Count) return true;

    //        for (int i = 0; i < FilterDefinitions.Count; i++)
    //        {
    //            if (FilterDefinitions[i].Title != _filterDataWrappers[i].Title)
    //                return true;

    //            if (FilterDefinitions[i].Operator != _filterDataWrappers[i].Operator)
    //                return true;

    //            if (FilterDefinitions[i].Value != _filterDataWrappers[i].Value)
    //                return true;
    //        }
    //    }

    //    return false;
    //}

    //private async Task SaveFilters()
    //{
    //    _filterDataWrappers.Clear();

    //    foreach (IFilterDefinition<T> filterDefinition in FilterDefinitions)
    //    {
    //        Console.WriteLine($"Save {filterDefinition.Value?.GetType()}");
    //        _filterDataWrappers.Add(new FilterDataWrapper()
    //        {
    //            ColumnIndex = RenderedColumns.FindIndex(column => column == filterDefinition.Column),
    //            Guid        = filterDefinition.Id,
    //            Operator    = filterDefinition.Operator,
    //            Title       = filterDefinition.Title,
    //            Value       = filterDefinition.Value,
    //        });
    //    }

    //    await LocalStorage.SetItemAsync(LocalStorageKey, _filterDataWrappers);
    //}

    //private async Task LoadFilters()
    //{
    //    _filterDataWrappers = await LocalStorage.GetItemAsync<List<FilterDataWrapper>>(LocalStorageKey) ?? _filterDataWrappers;

    //    foreach (FilterDataWrapper wrapper in _filterDataWrappers)
    //    {
    //        Console.WriteLine($"Load {wrapper.Value?.GetType()}");
    //        await AddFilterAsync(new FilterDefinition<T>()
    //        {
    //            Column = RenderedColumns.ElementAt(wrapper.ColumnIndex),
    //            Id = wrapper.Guid,
    //            Operator = wrapper.Operator,
    //            Title = wrapper.Title,
    //            Value = wrapper.Value,
    //        });
    //    }

    //    if (_filterDataWrappers.Count > 0)
    //        ToggleFiltersMenu();
    //}
}