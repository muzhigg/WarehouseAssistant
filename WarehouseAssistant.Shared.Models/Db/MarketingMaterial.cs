using Supabase.Postgrest.Models;

namespace WarehouseAssistant.Shared.Models.Db;

public class MarketingMaterial : BaseModel
{
    public string   Article      { get; set; } = null!;
    public string   Name         { get; set; } = null!;
    public string[] PackArticles { get; set; } = null!;
}