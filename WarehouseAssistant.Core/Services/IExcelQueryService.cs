using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

namespace WarehouseAssistant.Core.Services;

public interface IExcelQueryService
{
	Task<IEnumerable<dynamic>> QueryAsync(Stream stream, ExcelType excelType);

	IEnumerable<T> Query<T>(Stream stream,
		ExcelType                  excelType,
		OpenXmlConfiguration       configuration) where T : class, new();
}