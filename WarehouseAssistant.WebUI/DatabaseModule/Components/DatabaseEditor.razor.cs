using Microsoft.AspNetCore.Components;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.DatabaseModule.Components;

public partial class DatabaseEditor<TModel, TRepo> : ComponentBase where TModel : class
    where TRepo : IRepository<TModel>
{
    [Inject] private TRepo Repository { get; set; }
    private List<TModel> _models = new List<TModel>();


}