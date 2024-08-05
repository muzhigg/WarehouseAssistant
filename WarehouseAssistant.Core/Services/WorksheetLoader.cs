using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using MiniExcelLibs.OpenXml;
using WarehouseAssistant.Core.Calculation;

namespace WarehouseAssistant.Core.Services;

/// <summary>
///     Класс для загрузки и обработки данных из Excel файла.
/// </summary>
/// <param name="stream">Поток данных Excel файла</param>
public sealed class WorksheetLoader<TTableItem>(Stream stream) : IDisposable, IAsyncDisposable where TTableItem : class, ITableItem, new()
{
    /// <summary>
    ///     Класс для загрузки и обработки данных из Excel файла.
    /// </summary>
    /// <param name="path">Путь к Excel файлу.</param>
    public WorksheetLoader(string path) : this(File.OpenRead(path)) { }

    /// <summary>
    ///     Возвращает словарь, содержащий названия колонок и их значения из первой строки Excel файла.
    /// </summary>
    /// <returns>Словарь, где ключ - это буква колонки, а значение - её название.</returns>
    public Dictionary<string, string?> GetColumns()
    {
        return GetColumnsInternal().Result;
    }

    /// <summary>
    ///     Асинхронно возвращает словарь, содержащий названия колонок и их значения из первой строки Excel файла.
    /// </summary>
    /// <returns>Словарь, где ключ - это буква колонки, а значение - её название.</returns>
    public async Task<Dictionary<string, string?>> GetColumnsAsync()
    {
        return await GetColumnsInternal();
    }

    private async Task<Dictionary<string, string?>> GetColumnsInternal()
    {
        Dictionary<string, string?>  result   = [];
        IEnumerable<dynamic>?        query    = await stream.QueryAsync(excelType: ExcelType.XLSX);

        if (query.FirstOrDefault() is IDictionary<string, object> firstRow)
        {
            List<string>  columnLetters = [..firstRow.Keys];
            List<string?> columnValues  = firstRow.Values.Select(o => o as string).ToList();

            for (int i = 0; i < columnLetters.Count; i++)
                result.Add(columnLetters[i], columnValues[i]);
        }

        return result;
    }

    public IEnumerable<TTableItem> ParseItems()
    {
        return ParseItems(null);
    }

    /// <summary>
    ///     Парсит данные из Excel файла и возвращает коллекцию объектов типа T.
    /// </summary>
    /// <param name="selectedColumns">Выбранные динамические колонки для конфигурации парсинга.</param>
    /// <returns>Коллекция объектов типа T.</returns>
    public IEnumerable<TTableItem> ParseItems(DynamicExcelColumn[]? selectedColumns)
    {
        OpenXmlConfiguration configuration = new()
        {
            DynamicColumns = selectedColumns
        };

        return stream.Query<TTableItem>(excelType: ExcelType.XLSX, configuration: configuration)
            .Where(item => item.HasValidName() && item.HasValidArticle());
    }

    public void Dispose()
    {
        stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await stream.DisposeAsync();
    }
}