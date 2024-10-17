using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.Dialogs
{
    public partial class ProductFormDialog : ComponentBase
    {
        public static async Task<bool> ShowAddDialogAsync(ProductTableItem productTableItem,
            IDialogService                                                 dialogService)
        {
            Product product = new()
            {
                Article = productTableItem.Article,
                Name    = productTableItem.Name,
            };
            
            bool success = await ShowAddDialogAsync(product, dialogService);
            
            if (success)
                productTableItem.DbReference = product;
            
            return success;
        }
        
        public static async Task<bool> ShowAddDialogAsync(Product product,
            IDialogService                                        dialogService)
        {
            DialogParameters<ProductFormDialog> parameters = [];
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            parameters.Add(productDialog => productDialog.IsEditMode, false);
            
            IDialogReference? dialog = await dialogService.ShowAsync<ProductFormDialog>("Добавить товар", parameters);
            DialogResult?     result = await dialog.Result;
            
            return result.Canceled == false || result.Data is true;
        }
        
        public static async Task<Product?> ShowAddDialogAsync(IDialogService dialogService)
        {
            Product product = new();
            
            bool success = await ShowAddDialogAsync(product, dialogService);
            
            return success ? product : null;
        }
        
        public static async Task<bool> ShowEditDialogAsync(Product product, IDialogService dialogService)
        {
            DialogParameters<ProductFormDialog> parameters = [];
            
            parameters.Add(productDialog => productDialog.IsEditMode, true);
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            
            IDialogReference? dialog =
                await dialogService.ShowAsync<ProductFormDialog>("Редактировать товар", parameters);
            DialogResult? result = await dialog.Result;
            
            return result.Canceled == false || (bool)result.Data;
        }
        
        [Inject]    private IRepository<Product> Db         { get; set; } = null!;
        [Inject]    private ISnackbar            Snackbar   { get; set; } = null!;
        [Parameter] public  bool                 IsEditMode { get; set; }
        
        [Parameter] public Product EditedProduct { get; set; } = null!;
        
        [CascadingParameter] private MudDialogInstance? MudDialog { get; set; }
        
        public string? Article { get; set; }
        
        public string? ProductName { get; set; }
        
        public long? Barcode { get; set; }
        
        public int? QuantityPerBox { get; set; }
        
        public int? QuantityPerShelf { get; set; }
        
        private bool    _isValid;
        private MudForm _form = null!;
        
        protected override Task OnInitializedAsync()
        {
            if (Db.CanWrite == false)
                Snackbar.Add("Нет доступа для записи в базу данных", Severity.Error);
            
            return Task.CompletedTask;
        }
        
        protected override void OnParametersSet()
        {
            if (EditedProduct is null)
                throw new ArgumentException("EditedProduct is null");
            
            Article          = EditedProduct.Article;
            ProductName      = EditedProduct.Name;
            Barcode          = EditedProduct.Barcode;
            QuantityPerBox   = EditedProduct.QuantityPerBox;
            QuantityPerShelf = EditedProduct.QuantityPerShelf;
        }
        
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
                _form.Validate();
        }
        
        private async Task Submit()
        {
            EditedProduct.Article          = Article;
            EditedProduct.Name             = ProductName;
            EditedProduct.Barcode          = Barcode;
            EditedProduct.QuantityPerBox   = QuantityPerBox;
            EditedProduct.QuantityPerShelf = QuantityPerShelf;
            
            try
            {
                if (IsEditMode)
                    await Db.UpdateAsync(EditedProduct);
                else
                    await Db.AddAsync(EditedProduct);
                
                Snackbar.Add($"Товар {(IsEditMode ? "обновлен" : "добавлен")}" +
                             $"\n{EditedProduct.Article}" +
                             $"\n{EditedProduct.Name}", Severity.Success);
                MudDialog?.Close(true);
            }
            catch (HttpRequestException e)
            {
                Snackbar.Add($"Ошибка при сохранении товара {e.Message}", Severity.Error);
                MudDialog?.Close(false);
            }
        }
        
        private void Cancel()
        {
            MudDialog?.Close(DialogResult.Cancel());
        }
        
        internal async Task<string> ArticleValidation(string arg)
        {
            if (IsEditMode) return null!;
            
            if (string.IsNullOrEmpty(arg)) return "Артикул обязателен";
            
            if (StartsAndEndsWithNonWhitespaceChar(arg) == false) return "Артикул не должен содержать пробелы";
            
            if (await Db.GetByArticleAsync(arg) != null) return "Товар с данным артикулом существует";
            
            return null!;
        }
        
        public static bool StartsAndEndsWithNonWhitespaceChar(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            return !char.IsWhiteSpace(input[0]) && !char.IsWhiteSpace(input[^1]);
        }
    }
}