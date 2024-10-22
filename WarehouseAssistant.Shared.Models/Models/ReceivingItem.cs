using System.Diagnostics.CodeAnalysis;
using MiniExcelLibs.Attributes;

namespace WarehouseAssistant.Shared.Models;

public class ReceivingItem : ITableItem
{
    [NotNull, ExcelColumn(Name = "Товар")] public string Name { get; set; } = string.Empty;
    
    [NotNull, ExcelColumn(Name = "Артикул")]
    public string Article { get; set; } = string.Empty;
    
    [ExcelColumn(Name = "Количество")] public int ExpectedQuantity { get; set; }
    [ExcelIgnore]                      public int ReceivedQuantity { get; set; }
    
    public bool HasValidName()
    {
        return !string.IsNullOrEmpty(Name);
    }
    
    public bool HasValidArticle()
    {
        return ExpectedQuantity > 0;
    }
    
    public bool MatchesSearchString(string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
            return true;
        
        return Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
               Article.Contains(searchString, StringComparison.OrdinalIgnoreCase);
    }
}