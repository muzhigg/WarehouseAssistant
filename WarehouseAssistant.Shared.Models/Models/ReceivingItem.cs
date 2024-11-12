using System.Diagnostics.CodeAnalysis;
using MiniExcelLibs.Attributes;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace WarehouseAssistant.Shared.Models;

[Table("ReceivingItem")]
public class ReceivingItem : BaseModel, ITableItem
{
    [NotNull, ExcelColumn(Name = "Товар"), Column]
    public string Name { get; set; } = string.Empty;
    
    [NotNull, ExcelColumn(Name = "Артикул"), PrimaryKey]
    public string Article { get; set; } = string.Empty;
    
    [ExcelColumn(Name = "Количество"), Column]
    public int ExpectedQuantity { get; set; }
    
    [ExcelIgnore, Column] public int ReceivedQuantity { get; set; }
    
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