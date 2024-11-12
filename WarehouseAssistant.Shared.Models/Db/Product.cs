using System.Diagnostics.CodeAnalysis;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace WarehouseAssistant.Shared.Models.Db;

[Table("Products")]
public class Product : BaseModel, ITableItem
{
    [NotNull, PrimaryKey] public string? Article { get; set; }
    
    public bool HasValidName()
    {
        return true;
    }
    
    public bool HasValidArticle()
    {
        return true;
    }
    
    public bool MatchesSearchString(string searchString)
    {
        return Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
               || Article.Contains(searchString, StringComparison.InvariantCultureIgnoreCase);
    }
    
    [NotNull, Column] public string? Name             { get; set; }
    [Column]          public string? Barcode          { get; set; }
    [Column]          public int?    QuantityPerBox   { get; set; }
    [Column]          public int?    QuantityPerShelf { get; set; }
}