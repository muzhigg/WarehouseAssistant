using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WarehouseAssistant.WebUI.Components;

[CascadingTypeParameter(nameof(T))]
public class DataGrid<T> : MudDataGrid<T>
{
    private sealed class FilterDataWrapper
    {
        public int     ColumnIndex { get; init; }
        public Guid    Guid        { get; init; }
        public string? Title       { get; init; }
        public string? Operator    { get; init; }
        public object? Value       { get; init; }
    }

    [Parameter] public string LocalStorageKey { get; set; } = "DataGrid";

    [Inject] private ISyncLocalStorageService? LocalStorage { get; set; }

    private          List<FilterDataWrapper> _filterDataWrappers = [];
    private readonly object                  _filterLock         = new();

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            LoadFilters();
        }
        else
        {
            if (IsFiltersChanged()) SaveFilters();
        }
    }

    private bool IsFiltersChanged()
    {
        lock (_filterLock)
        {
            if (_filterDataWrappers.Count != FilterDefinitions.Count) return true;

            for (int i = 0; i < FilterDefinitions.Count; i++)
            {
                if (FilterDefinitions[i].Title != _filterDataWrappers[i].Title)
                    return true;

                if (FilterDefinitions[i].Operator != _filterDataWrappers[i].Operator)
                    return true;

                if (FilterDefinitions[i].Value != _filterDataWrappers[i].Value)
                    return true;
            }
        }

        return false;
    }

    private void SaveFilters()
    {
        lock (_filterLock)
        {
            _filterDataWrappers.Clear();

            foreach (IFilterDefinition<T> filterDefinition in FilterDefinitions)
            {
                _filterDataWrappers.Add(new FilterDataWrapper()
                {
                    ColumnIndex = RenderedColumns.FindIndex(column => column == filterDefinition.Column),
                    Guid        = filterDefinition.Id,
                    Operator    = filterDefinition.Operator,
                    Title       = filterDefinition.Title,
                    Value       = filterDefinition.Value,
                });
            }

            LocalStorage?.SetItem(LocalStorageKey, _filterDataWrappers);
        }
    }

    private void LoadFilters()
    {
        lock (_filterLock)
        {
            _filterDataWrappers = LocalStorage?.GetItem<List<FilterDataWrapper>>(LocalStorageKey) ?? new List<FilterDataWrapper>();

            foreach (FilterDataWrapper wrapper in _filterDataWrappers)
            {
                AddFilterAsync(new FilterDefinition<T>()
                {
                    Column   = RenderedColumns.ElementAt(wrapper.ColumnIndex),
                    Id       = wrapper.Guid,
                    Operator = wrapper.Operator,
                    Title    = wrapper.Title,
                    Value    = wrapper.Value,
                });
            }

            if (_filterDataWrappers.Count > 0)
                ToggleFiltersMenu();
        }
    }
}