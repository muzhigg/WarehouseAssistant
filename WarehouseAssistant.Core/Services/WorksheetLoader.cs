using MiniExcelLibs;

namespace WarehouseAssistant.Core.Services;

/// <summary>
/// Класс для загрузки и обработки данных из Excel файла.
/// </summary>
/// <param name="path">Путь к Excel файлу.</param>
public class WorksheetLoader(string path) : IDisposable
{
    private readonly FileStream _fileStream = File.OpenRead(path);

    /// <summary>
    /// Возвращает словарь, содержащий названия колонок и их значения из первой строки Excel файла.
    /// </summary>
    /// <returns>Словарь, где ключ - это буква колонки, а значение - её имя.</returns>
    public Dictionary<string, string?> GetColumns()
    {
        Dictionary<string, string?> result = [];

        List<string>        columnLetters       = [.. _fileStream.GetColumns(excelType: ExcelType.XLSX)];
        using MiniExcelDataReader excelDataReader = _fileStream.GetReader(excelType: ExcelType.XLSX);
        excelDataReader.Read();

        for (int i = 0; i < columnLetters.Count; i++) result.Add(columnLetters[i], excelDataReader.GetValue(i).ToString());

        return result;
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }
}