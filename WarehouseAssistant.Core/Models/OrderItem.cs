using MiniExcelLibs.Attributes;

namespace WarehouseAssistant.Core.Calculation;

public class OrderItem
{
    [ExcelColumn(Name = "Артикул")]    public string Article  { get; set; } = null!;
    [ExcelColumn(Name = "Количество")] public int    Quantity { get; set; }
}