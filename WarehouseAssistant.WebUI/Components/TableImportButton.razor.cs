using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.WebUI.Components;

public partial class TableImportButton<TTableItem> : MudComponentBase
    where TTableItem : class, Shared.Models.ITableItem, new()
{
    private bool          _isDialogVisible;
    private IBrowserFile? _selectedFile;
    private bool          _isLoading;
    
    /// <summary>
    /// Stores columns in the format {Column Letter, Column Name}
    /// </summary>
    private Dictionary<string, string?> _columns = new();
    
    /// <summary>
    /// Stores selected column mappings for each property of TTableItem
    /// </summary>
    private Dictionary<string, string?> _selectedColumns = new();
    
    private bool     _isValid;
    private MudForm? _form;
    
    /// <summary>
    /// Cache for properties of TTableItem that meet the required conditions
    /// </summary>
    private PropertyInfo[] _tableItemProperties = null!;
    
    [Parameter] public EventCallback<List<TTableItem>> OnParsed { get; set; }
    [Parameter] public bool                            Disabled { get; set; }
    private            bool                            _hidden;
    
    public override Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue("hidden", out bool hidden)) _hidden = hidden;
        
        return base.SetParametersAsync(parameters);
    }
    
    protected override void OnInitialized()
    {
        _tableItemProperties = typeof(TTableItem).GetProperties()
            .Where(p => TryGetExcelColumnAttribute(p, out _))
            .ToArray();
        InitializeSelectedColumns();
        
#if DEBUG
        Debug.WriteLine(
            $"[TableImportButton<{typeof(TTableItem).Name}>] Found {_tableItemProperties.Length} properties with ExcelColumnAttribute:");
        Debug.IndentLevel++;
        foreach (PropertyInfo property in _tableItemProperties)
        {
            TryGetExcelColumnAttribute(property, out ExcelColumnAttribute? columnAttr);
            Debug.WriteLine($"- {property.Name}: {columnAttr!.Name}");
        }
        
        Debug.IndentLevel--;
#endif
    }
    
    private void InitializeSelectedColumns()
    {
        foreach (var propertyInfo in _tableItemProperties)
            _selectedColumns.Add(propertyInfo.Name, null);
    }
    
    private void OpenDialog(MouseEventArgs obj)
    {
        _isDialogVisible = true;
    }
    
    private void CancelDialog()
    {
        _isDialogVisible = false;
        ResetValues();
    }
    
    private void ResetValues()
    {
        _selectedFile = null;
        _columns.Clear();
        _selectedColumns.Clear();
        InitializeSelectedColumns();
        _form?.ResetAsync();
        _isValid = false;
        StateHasChanged();
    }
    
    private async Task OnFilesChanged(IBrowserFile? obj)
    {
        if (obj == null)
            return;
        
        _isLoading = true;
        
        _selectedFile = obj;
        
        await using MemoryStream                memoryStream = await CopyFileToMemoryStream(_selectedFile);
        await using WorksheetLoader<TTableItem> sheetLoader  = new(memoryStream);
        
        _columns = await sheetLoader.GetColumnsAsync();
        
        MatchColumnsWithPropertiesUsingHeaderRow();
        
        if (_selectedColumns.Values.Any(string.IsNullOrEmpty))
            await MatchColumnsWithPropertiesAsync(sheetLoader);
        
        CheckFormValidity();
        _isLoading = false;
        StateHasChanged();
    }
    
    private async Task<MemoryStream> CopyFileToMemoryStream(IBrowserFile file)
    {
        var             memoryStream = new MemoryStream();
        await using var fileStream   = file.OpenReadStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
    
    private async Task MatchColumnsWithPropertiesAsync(WorksheetLoader<TTableItem> sheetLoader)
    {
        foreach (PropertyInfo propertyInfo in _tableItemProperties)
        {
            ExcelColumnAttribute? columnAttr = propertyInfo.GetCustomAttribute<ExcelColumnAttribute>();
            
            string? columnLetter = await sheetLoader.FindColumnLetterAsync(columnAttr!.Name);
            
            if (columnAttr.Aliases != null)
            {
                if (string.IsNullOrEmpty(columnLetter))
                {
                    foreach (string alias in columnAttr.Aliases)
                    {
                        columnLetter = await sheetLoader.FindColumnLetterAsync(alias);
                        if (columnLetter is not null) break;
                    }
                }
            }
            
            if (columnLetter is not null)
            {
                _selectedColumns[propertyInfo.Name] = columnLetter;
                _columns[columnLetter]              = columnAttr.Name;
            }
        }
    }
    
    
    private void MatchColumnsWithPropertiesUsingHeaderRow()
    {
        foreach (PropertyInfo propertyInfo in _tableItemProperties)
        {
            ExcelColumnAttribute? columnAttr = propertyInfo.GetCustomAttribute<ExcelColumnAttribute>();
            
            // Try to match the column name with the property name or its aliases
            KeyValuePair<string, string?> matchedColumn = _columns.FirstOrDefault(c =>
                c.Value != null && (
                    c.Value.Equals(columnAttr!.Name, StringComparison.OrdinalIgnoreCase) ||
                    (columnAttr.Aliases != null && columnAttr.Aliases.Any(alias =>
                        alias.Equals(c.Value, StringComparison.OrdinalIgnoreCase)))
                ));
            
            // If a match is found, set the matched column letter
            if (!string.IsNullOrEmpty(matchedColumn.Key))
                _selectedColumns[propertyInfo.Name] = matchedColumn.Key;
        }
    }
    
    private async Task ImportFile()
    {
        if (_selectedFile == null || _selectedColumns.Values.Any(string.IsNullOrEmpty)) return;
        
        _isLoading = true;
        
        await using var memoryStream = await CopyFileToMemoryStream(_selectedFile);
        await using var sheetLoader  = new WorksheetLoader<TTableItem>(memoryStream);
        
        // Create mapping configuration using DynamicExcelColumn
        var selectedColumns = _selectedColumns.Select(kvp => new DynamicExcelColumn(kvp.Key) { IndexName = kvp.Value })
            .ToArray();
        
        // Parse items using the selected column mapping
        var tableItems = new List<TTableItem>();
        foreach (TTableItem item in sheetLoader.ParseItems(selectedColumns))
        {
            if (item.HasValidName() && item.HasValidArticle())
                tableItems.Add(item);
        }
        
        // Invoke the OnParsed event callback with the parsed items
        await OnParsed.InvokeAsync(tableItems);
        
        _isLoading = false;
        
        // Close the dialog
        _isDialogVisible = false;
    }
    
    private void CheckFormValidity()
    {
        _isValid = _selectedColumns.Values.All(value => !string.IsNullOrEmpty(value));
    }
    
    private bool TryGetExcelColumnAttribute(PropertyInfo propertyInfo, out ExcelColumnAttribute? columnAttr)
    {
        columnAttr = propertyInfo.GetCustomAttribute<ExcelColumnAttribute>();
        return columnAttr is { Ignore: false } && propertyInfo.GetCustomAttribute<ExcelIgnoreAttribute>() == null;
    }
}