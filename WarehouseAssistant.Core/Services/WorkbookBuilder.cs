using MiniExcelLibs;

namespace WarehouseAssistant.Core.Services;

// Должен создавать и сохранять таблицы на диске.
public sealed class WorkbookBuilder<T> : IDisposable, IAsyncDisposable
{
    private readonly MemoryStream                _workbookStream = new();
    private readonly Dictionary<string, List<T>> _sheets         = [];

    public byte[] AsByteArray()
    {
        return AsByteArray(null);
    }

    public byte[] AsByteArray(IConfiguration? configuration)
    {
        Dictionary<string, object> sheets = _sheets.ToDictionary<KeyValuePair<string, List<T>>, string, object>(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);

        _workbookStream.SaveAs(sheets, configuration: configuration);
        return _workbookStream.ToArray();
    }

    public bool CreateSheet(string sheetName)
    {
        if (_sheets.ContainsKey(sheetName))
            return false;

        _sheets.Add(sheetName, []);
        return true;
    }

    public void AddToSheet(string sheetName, T data)
    {
        _sheets[sheetName].Add(data);
    }

    public void AddRangeToSheet(string sheetName, IEnumerable<T> data)
    {
        _sheets[sheetName].AddRange(data);
    }

    public void Dispose()
    {
        _workbookStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _workbookStream.DisposeAsync();
    }
}