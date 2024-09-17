using MiniExcelLibs;

namespace WarehouseAssistant.Core.Services;

/// <summary>
/// This class is used to build a workbook with multiple sheets and add data to each sheet.
/// </summary>
/// <typeparam name="T">The type of data to be added to the workbook.</typeparam>
public sealed class WorkbookBuilder<T> : IDisposable, IAsyncDisposable
{
    private readonly MemoryStream                _workbookStream = new();
    private readonly Dictionary<string, List<T>> _sheets         = [];
    
    /// <summary>
    /// Gets the data from the specified sheet.
    /// </summary>
    /// <param name="sheetName">The name of the sheet from which to retrieve the data.</param>
    /// <returns>
    /// A list of data from the specified sheet, or <c>null</c> if the sheet does not exist.
    /// </returns>
    public List<T>? GetSheetData(string sheetName)
    {
        return _sheets.GetValueOrDefault(sheetName);
    }

    
    /// <summary>
    /// Returns the workbook as a byte array without additional configuration.
    /// </summary>
    /// <returns> The workbook as a byte array. </returns>
    public byte[] AsByteArray()
    {
        return AsByteArray(null);
    }

    /// <summary>
    /// Returns the workbook as a byte array with additional configuration.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns> The workbook as a byte array. </returns>
    public byte[] AsByteArray(IConfiguration? configuration)
    {
        _workbookStream.Position = 0;
        Dictionary<string, object> sheets = _sheets.ToDictionary<KeyValuePair<string, List<T>>, string, object>(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);

        _workbookStream.SaveAs(sheets, configuration: configuration);
        return _workbookStream.ToArray();
    }

    /// <summary>
    /// Creates a new sheet with the specified name.
    /// </summary>
    /// <param name="sheetName"> The name of the sheet to be created. </param>
    /// <returns> True if the sheet was created successfully, false otherwise. </returns>
    public bool CreateSheet(string sheetName)
    {
        if (_sheets.ContainsKey(sheetName))
            return false;

        _sheets.Add(sheetName, []);
        return true;
    }

    /// <summary>
    /// Adds data to the specified sheet.
    /// </summary>
    /// <param name="sheetName"> The name of the sheet to which the data should be added. </param>
    /// <param name="data"> The data to be added to the sheet. </param>
    public void AddToSheet(string sheetName, T data)
    {
        _sheets[sheetName].Add(data);
    }

    /// <summary>
    /// Adds a range of data to the specified sheet.
    /// </summary>
    /// <param name="sheetName"> The name of the sheet to which the data should be added. </param>
    /// <param name="data"> The data to be added to the sheet. </param>
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