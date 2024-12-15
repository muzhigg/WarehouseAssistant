using Microsoft.AspNetCore.Components.Forms;

namespace WarehouseAssistant.WebUI.Utils;

internal static class BrowserFileExtensions
{
    // This method is needed because BrowserFileStream does not support Seeking
    public static async Task<MemoryStream> ConvertBrowserFileToMemoryStream(this IBrowserFile browserFile)
    {
        MemoryStream       memoryStream = new MemoryStream();
        await using Stream fileStream   = browserFile.OpenReadStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}