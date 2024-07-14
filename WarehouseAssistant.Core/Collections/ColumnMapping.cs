namespace WarehouseAssistant.Core.Collections;

/// <summary>
/// Класс для управления сопоставлением ключей и букв столбцов в таблице.
/// </summary>
public class ColumnMapping
{
    // Предопределенные ключи
    public const string NameKey = "Номенклатура";
    public const string ArticleKey = "Артикул";
    public const string AvailableQuantityKey = "Доступно основной склад МО";
    public const string CurrentQuantityKey = "Доступно Санкт-Петербург (склад)";
    public const string ReservedKey = "В резерве СПб";
    public const string AverageTurnoverKey = "Средняя оборачиваемость в день";
    public const string StockDaysKey = "Запас товара (на кол-во дней)";
    public const string OrderCalculationKey = "Расчет заказа";
    public const string OrderAmountKey = "Заказ на офис Спб";

    // Словарь для хранения соответствий ключ-буква_столбца
    private readonly Dictionary<string, string?> _columnMappings = new();

    /// <summary>
    /// Индексатор для доступа к буквам столбцов по ключам.
    /// </summary>
    /// <param name="key">Ключ сопоставления.</param>
    /// <returns>Буква столбца или null.</returns>
    public string? this[string key]
    {
        get => GetColumnLetter(key);
        set => UpdateMapping(key, value);
    }

    /// <summary>
    /// Метод для добавления новой пары ключ-буква_столбца.
    /// </summary>
    /// <param name="key">Ключ сопоставления.</param>
    /// <param name="columnLetter">Буква столбца.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если ключ уже существует.</exception>
    public void AddMapping(string key, string? columnLetter)
    {
        if (!_columnMappings.TryAdd(key, columnLetter))
            throw new ArgumentException("Этот ключ уже существует в коллекции.", nameof(key));
    }

    /// <summary>
    /// Метод для удаления пары ключ-буква_столбца.
    /// </summary>
    /// <param name="key">Ключ сопоставления.</param>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если ключ не найден.</exception>
    public void RemoveMapping(string key)
    {
        if (!_columnMappings.Remove(key)) throw new KeyNotFoundException("Этот ключ не найден в коллекции.");
    }

    /// <summary>
    /// Метод для получения буквы столбца по ключу.
    /// </summary>
    /// <param name="key">Ключ сопоставления.</param>
    /// <returns>Буква столбца или null.</returns>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если ключ не найден.</exception>
    public string? GetColumnLetter(string key)
    {
        return _columnMappings.TryGetValue(key, out string? columnLetter)
            ? columnLetter
            : throw new KeyNotFoundException("Этот ключ не найден в коллекции.");
    }

    /// <summary>
    /// Метод для обновления буквы столбца по ключу.
    /// </summary>
    /// <param name="key">Ключ сопоставления.</param>
    /// <param name="newColumnLetter">Новая буква столбца.</param>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если ключ не найден.</exception>
    public void UpdateMapping(string key, string? newColumnLetter)
    {
        if (!_columnMappings.ContainsKey(key))
            throw new KeyNotFoundException("Этот ключ не найден в коллекции.");

        _columnMappings[key] = newColumnLetter;
    }
}