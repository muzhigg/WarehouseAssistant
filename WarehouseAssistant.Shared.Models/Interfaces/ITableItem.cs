namespace WarehouseAssistant.Shared.Models;

public interface ITableItem
{
    public string Name    { get; set; }
    public string Article { get; set; }
    
    public bool HasValidName();
    
    public bool HasValidArticle();
}