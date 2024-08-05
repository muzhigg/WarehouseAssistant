using System.Collections;

namespace WarehouseAssistant.Core.Collections;

/// <summary>
/// Класс для управления сопоставлением ключей и букв столбцов в таблице.
/// </summary>
public class ColumnMapping : IEnumerable<KeyValuePair<string, string?>>
{
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

    public bool HasColumn(string key)
    {
        return _columnMappings.ContainsKey(key) && _columnMappings[key] != null;
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
        return _columnMappings[key];

        //return _columnMappings.TryGetValue(key, out string? columnLetter)
        //    ? columnLetter
        //    : throw new KeyNotFoundException($"Этот ключ не найден в коллекции. ({key})");
    }

    public int GetColumnIndex(string key)
    {
        string? columnLetter = GetColumnLetter(key);

        int columnIndex = 0;
        int factor      = 1;

        if (columnLetter == null) return columnIndex;

        for (int i = columnLetter.Length - 1; i >= 0; i--)
        {
            char letter      = columnLetter[i];
            int  letterValue = letter - 'A' + 1;
            columnIndex += letterValue * factor;
            factor      *= 26;
        }

        return columnIndex;
    }

    public bool TryUpdateMapping(string key, string? columnLetter)
    {
        if (!_columnMappings.ContainsKey(key)) return false;

        _columnMappings[key] = columnLetter;
        return true;
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
            throw new KeyNotFoundException($"Ключ {key} не найден в коллекции.");

        _columnMappings[key] = newColumnLetter;
    }

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return _columnMappings.GetEnumerator();
    }

    IEnumerator IEnumerable.                          GetEnumerator()
    {
        return GetEnumerator();
    }
}