using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Dialogs
{
    public partial class ProductFormDialog : ComponentBase
    {
        public static Task<Product?> ShowAddDialogAsync(ProductTableItem productTableItem,
            IDialogService                                               dialogService)
        {
            Product product = new()
            {
                Article = productTableItem.Article,
                Name    = productTableItem.Name,
            };
            
            return ShowAddDialogAsync(product, dialogService);
        }
        
        public static async Task<Product?> ShowAddDialogAsync(Product product,
            IDialogService                                            dialogService)
        {
            DialogParameters<ProductFormDialog> parameters = new();
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            parameters.Add(productDialog => productDialog.IsEditMode, false);
            
            IDialogReference? dialog = await dialogService.ShowAsync<ProductFormDialog>("Добавить товар", parameters);
            DialogResult?     result = await dialog.Result;
            
            if (!result.Canceled) return (Product)result.Data;
            
            return null;
        }
        
        public static async Task<Product?> ShowEditDialogAsync(Product product, IDialogService dialogService)
        {
            DialogParameters<ProductFormDialog> parameters = [];
            
            parameters.Add(productDialog => productDialog.IsEditMode, true);
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            
            var dialog =
                dialogService.Show<ProductFormDialog>("Редактировать товар", parameters);
            DialogResult? result = await dialog.Result;
            
            if (!result.Canceled) return (Product)result.Data;
            
            return null;
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
            }
            catch (HttpRequestException e)
            {
#if DEBUG
                Debug.WriteLine(e, "Error");
#endif
                Snackbar.Add("Ошибка при сохранении товара", Severity.Error);
                return;
            }
            
            MudDialog?.Close(EditedProduct);
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