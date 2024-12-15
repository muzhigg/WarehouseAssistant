using Microsoft.AspNetCore.Components.Forms;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.WebUI.ProductOrder;

namespace WarehouseAssistant.WebUI.ProductOrderModule.Dialogs;

public partial class
    SubtractFromTableStrategyDialog : BaseProductCalculatorDialog<SubtractFromTableStrategy, SubtractFromTableOptions>
{
    private IBrowserFile? _selectedFile;
    
    private async Task OnFilesChanged(IReadOnlyList<IBrowserFile>? files)
    {
        // Options.ClearTables();
        //
        // foreach (IBrowserFile browserFile in files)
        // {
        //     await Options.AddTableAsync(CopyFileToMemoryStream(browserFile));
        // }
    }
    
    private async Task<MemoryStream> CopyFileToMemoryStream(IBrowserFile file)
    {
        var             memoryStream = new MemoryStream();
        await using var fileStream   = file.OpenReadStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}