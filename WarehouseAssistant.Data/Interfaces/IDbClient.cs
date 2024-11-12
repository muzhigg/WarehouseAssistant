namespace WarehouseAssistant.Data.Interfaces;

public interface IDbClient
{
    void SetAuthBearer(string token);
}