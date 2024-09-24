using MiniExcelLibs.Attributes;

namespace WarehouseAssistant.Shared.Models;

public sealed class ProductTableItem : ICalculatedTableItem
{
    private int    _quantityToOrder;
    private int    _availableQuantity;
    private int    _currentQuantity;
    private double _averageTurnover;
    private double _stockDays;
    
    [ExcelColumn(Name = "Название", Aliases = ["Номенклатура"], Width = 70.0)]
    public string Name { get; set; } = string.Empty;
    
    [ExcelColumn(Name = "Артикул", Width = 10.0)]
    public string Article { get; set; } = string.Empty;
    
    [ExcelColumn(Name = "Доступно на БГЛЦ", Aliases = ["Доступно основной склад МО"], Width = 16.0)]
    public int AvailableQuantity
    {
        get => _availableQuantity;
        set => _availableQuantity = Math.Clamp(value, 0, int.MaxValue);
    }
    
    [ExcelColumn(Name = "Текущее количество", Aliases = ["Доступно Санкт-Петербург (склад)"], Width = 18.0)]
    public int CurrentQuantity
    {
        get => _currentQuantity;
        set => _currentQuantity = Math.Clamp(value, 0, int.MaxValue);
    }
    
    [ExcelColumn(Name = "Средний расход в день", Aliases = ["Средняя оборачиваемость в день"], Width = 16.0)]
    public double AverageTurnover
    {
        get => _averageTurnover;
        set => _averageTurnover = Math.Clamp(value, 0.0, double.MaxValue);
    }
    
    [ExcelColumn(Name = "Запас на кол-во дней)",
        Aliases = ["Запас товара (на кол-во дней)  ", "Запас товара (на кол-во дней)"], Width = 17.0)]
    public double StockDays
    {
        get => _stockDays;
        set => _stockDays = Math.Clamp(value, 0.0, double.MaxValue);
    }
    
    [ExcelColumn(Name = "Рекомендуемое количество", Aliases = ["Расчет заказа"], Width = 13.0)]
    public double OrderCalculation { get; set; }
    
    [ExcelColumn(Name = "Нужно заказать", Width = 16.0, Aliases = ["Заказ на офис Спб"])]
    public int QuantityToOrder
    {
        get => _quantityToOrder;
        set
        {
            int minCanBeOrdered = MaxCanBeOrdered;
            int val             = value > minCanBeOrdered ? minCanBeOrdered : value;
            _quantityToOrder = Math.Clamp(val, 0, int.MaxValue);
        }
    }
    
    [ExcelColumn(Ignore = true)] public int MaxCanBeOrdered => (int)Math.Floor(AvailableQuantity * 0.07);
    
    public bool HasValidName()
    {
        return !string.IsNullOrEmpty(Name) && !Name.Contains("акция", StringComparison.OrdinalIgnoreCase);
    }
    
    public bool HasValidArticle()
    {
        return !string.IsNullOrEmpty(Article) && Article.Length == 8 && Article.All(char.IsDigit);
    }
}