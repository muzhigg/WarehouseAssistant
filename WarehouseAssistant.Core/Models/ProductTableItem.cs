using MiniExcelLibs.Attributes;
using WarehouseAssistant.Core.Calculation;

namespace WarehouseAssistant.Core.Models;

public class ProductTableItem : ICalculationData
{
    private int _quantityToOrder;

    [ExcelColumn(Name = "Название", Aliases = ["Номенклатура"], Width = 70.0)]
    public required string Name { get; set; }

    [ExcelColumn(Name = "Артикул", Width = 10.0)]
    public required int Article { get; set; }

    [ExcelColumn(Name = "Доступно на БГЛЦ", Aliases = ["Доступно основной склад МО"], Width = 16.0)]
    public int AvailableQuantity { get; set; }

    [ExcelColumn(Name = "Текущее количество", Aliases = ["Доступно Санкт-Петербург (склад)"], Width = 18.0)]
    public int CurrentQuantity { get; set; }

    [ExcelIgnore]
    public int Reserved { get; set; }

    [ExcelColumn(Name = "Средний расход в день", Aliases = ["Средняя оборачиваемость в день"], Width = 16.0)]
    public double AverageTurnover { get; set; }

    [ExcelColumn(Name = "Запас товара (на кол-во дней)", Width = 17.0)]
    public double StockDays { get; set; }

    [ExcelColumn(Name = "Рекомендуемое количество", Aliases = ["Расчет заказа"], Width = 13.0)]
    public double OrderCalculation { get; set; }

    [ExcelColumn(Name = "Нужно заказать", Width = 16.0, Aliases = ["Заказ на офис Спб"])]
    public int QuantityToOrder
    {
        get => _quantityToOrder;
        set
        {
            int minCanBeOrdered = MinCanBeOrdered;
            _quantityToOrder = value > minCanBeOrdered ? minCanBeOrdered : value;
        }
    }

    private int MinCanBeOrdered => (int)Math.Floor(AvailableQuantity * 0.07);
}