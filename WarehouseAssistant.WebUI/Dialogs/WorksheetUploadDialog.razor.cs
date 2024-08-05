using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MiniExcelLibs.Attributes;
using MudBlazor;
using WarehouseAssistant.Core.Calculation;
using WarehouseAssistant.Core.Collections;
using WarehouseAssistant.Core.Services;

namespace WarehouseAssistant.WebUI.Dialogs
{
    public partial class WorksheetUploadDialog<TTableItem> : ComponentBase, IDisposable where TTableItem : class, ITableItem, new()
    {
        [Parameter]
        public DynamicExcelColumn[]? ExcelColumns { get; set; }

        public ColumnMapping RequiredColumns { get; } = new();

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;

        private MudForm?                     _form;
        private IBrowserFile?                _file;
        private WorksheetLoader<TTableItem>?          _loader;
        private Dictionary<string, string?>? _worksheetColumns;
        private bool                         _isValid;

        protected override void OnInitialized()
        {
            if (ExcelColumns != null)
                foreach (DynamicExcelColumn dynamicExcelColumn in ExcelColumns)
                    RequiredColumns.AddMapping(dynamicExcelColumn.Key, null);
        }

        private async Task HandleFileSelected(IBrowserFile? obj)
        {
            if (obj == null) return;

            CleanUp();
            _file = obj;
            MemoryStream       stream     = new();
            await using Stream fileStream = _file.OpenReadStream();
            await fileStream.CopyToAsync(stream);
            stream.Position = 0;  // Reset stream position to the beginning.
            _loader         = new WorksheetLoader<TTableItem>(stream);

            if (ExcelColumns != null)
            {
                _worksheetColumns = await _loader.GetColumnsAsync();
                MatchColumns();
            }
            else
            {
                _isValid = true;
            }

            StateHasChanged();
            if (_form != null) await _form.Validate();
        }

        private void MatchColumns()
        {
            if (_worksheetColumns == null || ExcelColumns == null) return;

            var columnMapping = _worksheetColumns
                .SelectMany(kvp => (kvp.Value?.Split(',') ?? []).Select(alias => new { kvp.Key, Alias = alias }))
                .ToDictionary(item => item.Alias, item => item.Key);

            foreach (var excelColumn in ExcelColumns)
            {
                PropertyInfo? prop = typeof(TTableItem).GetProperty(excelColumn.Key);
                if (prop == null) continue;

                var propAttribute = prop.GetCustomAttribute<ExcelColumnAttribute>();
                if (propAttribute == null) continue;

                if (columnMapping.TryGetValue(propAttribute.Name, out var columnLetter) ||
                    (propAttribute.Aliases != null && propAttribute.Aliases.Any(alias => columnMapping.TryGetValue(alias, out columnLetter))))
                    RequiredColumns.UpdateMapping(excelColumn.Key, columnLetter);
            }
        }

        private void CleanUp()
        {
            _worksheetColumns = null;
            _loader?.Dispose();
            _loader = null;
        }

        public void Dispose()
        {
            CleanUp();
        }

        private void OnSubmit(MouseEventArgs obj)
        {
            if (_loader == null) return;

            if (ExcelColumns != null)
                foreach (DynamicExcelColumn dynamicExcelColumn in ExcelColumns)
                    dynamicExcelColumn.IndexName = RequiredColumns.GetColumnLetter(dynamicExcelColumn.Key);

            IEnumerable<TTableItem> result = _loader.ParseItems(ExcelColumns).ToList();
            MudDialog.Close(DialogResult.Ok(result));
        }


        private void OnCancel(MouseEventArgs obj)
        {
            MudDialog.Cancel();
        }
    }
}
