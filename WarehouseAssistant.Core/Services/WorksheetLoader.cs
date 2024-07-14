using MiniExcelLibs;
using WarehouseAssistant.Core.Collections;
using WarehouseAssistant.Core.Models;

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

    /// <summary>
    /// Парсит данные из Excel файла и возвращает список объектов ProductTableItem.
    /// </summary>
    /// <param name="selectedColumns">Сопоставление ключей и букв столбцов.</param>
    /// <returns>Список объектов ProductTableItem.</returns>
    public List<ProductTableItem> ParseItems(ColumnMapping selectedColumns)
    {
        List<ProductTableItem> result = [];

        List<string?> columnLetters = [.. _fileStream.GetColumns(excelType: ExcelType.XLSX)];
        using MiniExcelDataReader reader = _fileStream.GetReader(true, excelType: ExcelType.XLSX);

        while (reader.Read())
        {
            string? productName = reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.NameKey])).ToString();

            if (string.IsNullOrEmpty(productName) || productName.Contains("акция", StringComparison.OrdinalIgnoreCase))
                continue;

            object articleFieldRawValue = reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.ArticleKey]));
            try
            {
                int article = ParseInt(articleFieldRawValue);

                if (!HasEightDigits(article)) continue;

                result.Add(new ProductTableItem
                {
                    Article = article,
                    Name = productName,
                    AvailableQuantity = ParseInt(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.AvailableQuantityKey]))),
                    AverageTurnover = ParseDouble(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.AverageTurnoverKey]))),
                    CurrentQuantity = ParseInt(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.CurrentQuantityKey]))),
                    Reserved = ParseInt(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.ReservedKey]))),
                    StockDays = ParseDouble(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.StockDaysKey]))),
                    OrderCalculation = ParseDouble(reader.GetValue(columnLetters.IndexOf(selectedColumns[ColumnMapping.OrderCalculationKey]))),
                });
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        return result;
    }


    private static bool HasEightDigits(int article)
    {
        return Math.Floor(Math.Log10(article) + 1) == 8;
    }


    private static int ParseInt(object value)
    {
        try
        {
            return Convert.ToInt32(value);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private static double ParseDouble(object value)
    {
        try
        {
            return Convert.ToDouble(value);
        }
        catch (Exception)
        {
            return 0.0;
        }
    }

    public void Dispose()
    {
        _fileStream.Dispose();
        GC.SuppressFinalize(this);
    }
}