using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using WarehouseAssistant.Core.Calculation;

namespace WarehouseAssistant.Core.Services;

public sealed class WorksheetLoader<TTableItem> : IDisposable, IAsyncDisposable
    where TTableItem : class, ITableItem, new()
{
    private readonly Stream             _stream;
    private readonly IExcelQueryService _excelQueryService;

    public WorksheetLoader(string path,
        IExcelQueryService        excelQueryService) : this(File.OpenRead(path), excelQueryService) { }

    public WorksheetLoader(Stream stream, IExcelQueryService excelQueryService)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanRead == false)
            throw new ArgumentException("Stream must be readable.", nameof(stream));

        _stream            = stream;
        _excelQueryService = excelQueryService ?? throw new ArgumentNullException(nameof(excelQueryService));
    }

    public WorksheetLoader(Stream stream) : this(stream, new ExcelQueryService()) { }

    public WorksheetLoader(string path) : this(path, new ExcelQueryService()) { }

    private async Task<Dictionary<string, string?>> GetColumnsInternal()
    {
        Dictionary<string, string?> result = [];
        IEnumerable<dynamic>        query  = await _excelQueryService.QueryAsync(_stream, ExcelType.XLSX);

        if (query.FirstOrDefault() is not IDictionary<string, object> firstRow) return result;

        foreach (KeyValuePair<string, object> kvp in firstRow) result[kvp.Key] = kvp.Value as string;

        return result;
    }

    /// <summary>
    /// Извлекает первую строку Excel-файла и возвращает словарь, где ключи — это буквы столбцов,
    /// а значения — это соответствующие значения из ячеек первой строки.
    /// </summary>
    /// <returns>
    /// Возвращает словарь, где ключи — это буквы столбцов (например, "A", "B", "C"), а значения — строки,
    /// содержащие данные из ячеек первой строки соответствующих столбцов.
    /// Если в файле Excel нет данных или первая строка не содержит допустимых данных, возвращается пустой словарь.
    /// </returns>
    public Dictionary<string, string?> GetColumns()
    {
        return GetColumnsAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Асинхронно извлекает первую строку Excel-файла и возвращает словарь, где ключи — это буквы столбцов,
    /// а значения — это соответствующие значения из ячеек первой строки.
    /// </summary>
    /// <returns>
    /// Возвращает словарь, где ключи — это буквы столбцов (например, "A", "B", "C"), а значения — строки,
    /// содержащие данные из ячеек первой строки соответствующих столбцов.
    /// Если в файле Excel нет данных или первая строка не содержит допустимых данных, возвращается пустой словарь.
    /// </returns>
    public async Task<Dictionary<string, string?>> GetColumnsAsync()
    {
        return await GetColumnsInternal();
    }

    public IEnumerable<TTableItem> ParseItems()
    {
        return ParseItems(null);
    }

    public IEnumerable<TTableItem> ParseItems(DynamicExcelColumn[]? selectedColumns)
    {
        OpenXmlConfiguration configuration = new()
        {
            DynamicColumns = selectedColumns,
        };
        
        return _excelQueryService.Query<TTableItem>(_stream, ExcelType.XLSX, configuration)
            .Where(item => item.HasValidName() && item.HasValidArticle());
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();
    }
}