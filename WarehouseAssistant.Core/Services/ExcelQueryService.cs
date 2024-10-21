using System.Reflection;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;

namespace WarehouseAssistant.Core.Services;

internal sealed class ExcelQueryService : IExcelQueryService
{
    public Task<IEnumerable<dynamic>> QueryAsync(Stream stream, ExcelType excelType)
    {
        return stream.QueryAsync(excelType: excelType);
    }
    
    public IEnumerable<T> Query<T>(Stream stream, ExcelType excelType, OpenXmlConfiguration configuration)
        where T : class, new()
    {
        return ParseFromReader<T>(stream, excelType, configuration);
    }
    
    private int GetColumnIndex(PropertyInfo info,                 DynamicExcelColumn?       dynamicExcelColumn,
        ExcelColumnAttribute?               excelColumnAttribute, ExcelColumnNameAttribute? columnNameAttr,
        Dictionary<string, int>             columns)
    {
        if (dynamicExcelColumn is { Index: not -1 }) return dynamicExcelColumn.Index;
        
        // Проверка по Aliases
        string[] aliases = dynamicExcelColumn?.Aliases
                           ?? excelColumnAttribute?.Aliases
                           ?? columnNameAttr?.Aliases
                           ?? [];
        
        foreach (string alias in aliases)
            if (columns.TryGetValue(alias, out int aliasIndex))
                return aliasIndex;
        
        // Проверка по Name
        string name = dynamicExcelColumn?.Name
                      ?? excelColumnAttribute?.Name
                      ?? columnNameAttr?.ExcelColumnName
                      ?? info.Name;
        
        if (columns.TryGetValue(name, out int nameIndex)) return nameIndex;
        
        // Проверка по Index
        return excelColumnAttribute?.Index
               ?? info.GetCustomAttribute<ExcelColumnIndexAttribute>()?.ExcelColumnIndex
               ?? -1;
    }
    
    private sealed class PropertyConfig
    {
        public PropertyInfo PropertyInfo { get; init; } = null!;
        public bool         Ignore       { get; init; }
        public int          Index        { get; init; }
    }
    
    private IEnumerable<PropertyConfig> GetPropertiesWithConfiguration<TTableItem>(OpenXmlConfiguration configuration,
        Dictionary<string, int> columns) where TTableItem : new()
    {
        return typeof(TTableItem).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(info => info.CanWrite)
            .Select(info =>
            {
                ExcelColumnAttribute? excelColumnAttribute = info.GetCustomAttribute<ExcelColumnAttribute>();
                DynamicExcelColumn? dynamicExcelColumn =
                    configuration.DynamicColumns?.FirstOrDefault(column => column.Key == info.Name);
                ExcelColumnNameAttribute? columnNameAttr = info.GetCustomAttribute<ExcelColumnNameAttribute>();
                
                bool ignore = excelColumnAttribute?.Ignore == true
                              || info.GetCustomAttribute<ExcelIgnoreAttribute>()?.ExcelIgnore == true
                              || dynamicExcelColumn?.Ignore == true;
                
                int index = GetColumnIndex(info, dynamicExcelColumn, excelColumnAttribute, columnNameAttr, columns);
                
                return new PropertyConfig
                {
                    PropertyInfo = info,
                    Ignore       = ignore || index == -1,
                    Index        = index
                };
            });
    }
    
    private void PopulateItem<TTableItem>(TTableItem item, MiniExcelDataReader reader,
        IEnumerable<PropertyConfig>                  props) where TTableItem : new()
    {
        foreach (PropertyConfig propertyData in props.Where(propertyData => !propertyData.Ignore))
            try
            {
                object value          = reader.GetValue(propertyData.Index);
                object convertedValue = Convert.ChangeType(value, propertyData.PropertyInfo.PropertyType);
                propertyData.PropertyInfo.SetValue(item, convertedValue);
            }
            catch (Exception)
            {
                object? defaultValue = propertyData.PropertyInfo.PropertyType.IsValueType
                    ? Activator.CreateInstance(propertyData.PropertyInfo.PropertyType)
                    : null;
                propertyData.PropertyInfo.SetValue(item, defaultValue);
            }
    }
    
    private IEnumerable<TTableItem> ParseFromReader<TTableItem>(Stream stream, ExcelType excelType,
        OpenXmlConfiguration                                           configuration) where TTableItem : new()
    {
        List<TTableItem> result = [];
        
        using MiniExcelDataReader reader = stream.GetReader(configuration: configuration, excelType: excelType);
        
        reader.Read();
        
        // first row cell is a key || column index is a value
        Dictionary<string, int> columns = GetColumnIndices(reader, configuration);
        
        PropertyConfig[] props = GetPropertiesWithConfiguration<TTableItem>(configuration, columns).ToArray();
        
        while (reader.Read())
        {
            TTableItem item = new();
            PopulateItem(item, reader, props);
            result.Add(item);
        }
        
        return result;
    }
    
    /// <summary>
    /// Get column indices from reader
    /// </summary>
    /// <returns>Dictionary of column name and index</returns>
    private Dictionary<string, int> GetColumnIndices(MiniExcelDataReader reader, OpenXmlConfiguration configuration)
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        foreach (int i in Enumerable.Range(0, reader.FieldCount))
        {
            if (configuration.DynamicColumns?.Any(column => column.Index == i) == true)
            {
                dictionary.Add(configuration.DynamicColumns.First(column => column.Index == i).Key, i);
                continue;
            }
            
            string key = reader.GetValue(i)?.ToString() ?? string.Empty;
            dictionary.TryAdd(key, i);
        }
        
        return dictionary;
    }
}