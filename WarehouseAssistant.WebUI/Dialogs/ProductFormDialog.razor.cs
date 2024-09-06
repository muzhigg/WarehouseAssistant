using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Dialogs
{
    public partial class ProductFormDialog : ComponentBase
    {
        public static async Task<Product?> ShowAddDialogAsync(ProductTableItem productTableItem,
            IDialogService                                                     dialogService)
        {
            Product product = new()
            {
                Article = productTableItem.Article,
                Name    = productTableItem.Name,
            };
            
            DialogParameters<ProductFormDialog> parameters = [];
            parameters.Add(productDialog => productDialog.IsEditMode, false);
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            
            IDialogReference? dialog = await dialogService.ShowAsync<ProductFormDialog>("Добавить товар", parameters);
            DialogResult?     result = dialog.Result.Result;
            
            if (!result.Canceled) return (Product)result.Data;
            
            return null;
        }
        
        public static async Task<Product?> ShowEditDialogAsync(Product product, IDialogService dialogService)
        {
            DialogParameters<ProductFormDialog> parameters = [];
            
            parameters.Add(productDialog => productDialog.IsEditMode, true);
            parameters.Add(productDialog => productDialog.EditedProduct, product);
            
            IDialogReference? dialog =
                await dialogService.ShowAsync<ProductFormDialog>("Редактировать товар", parameters);
            DialogResult? result = dialog.Result.Result;
            
            if (!result.Canceled) return (Product)result.Data;
            
            return null;
        }
        
        [Inject]    private IRepository<Product> Db         { get; set; } = null!;
        [Inject]    private ISnackbar            Snackbar   { get; set; } = null!;
        [Parameter] public  bool                 IsEditMode { get; set; }
        
        [Parameter] public Product EditedProduct { get; set; } = new();
        
        [CascadingParameter] private MudDialogInstance? MudDialog { get; set; }
        
        public string? Article
        {
            get => EditedProduct.Article;
            set => EditedProduct.Article = value;
        }
        
        public string? ProductName
        {
            get => EditedProduct.Name;
            set => EditedProduct.Name = value;
        }
        
        public long? Barcode
        {
            get => EditedProduct.Barcode;
            set => EditedProduct.Barcode = value;
        }
        
        public int? QuantityPerBox
        {
            get => EditedProduct.QuantityPerBox;
            set => EditedProduct.QuantityPerBox = value;
        }
        
        public int? QuantityPerShelf
        {
            get => EditedProduct.QuantityPerShelf;
            set => EditedProduct.QuantityPerShelf = value;
        }
        
        private bool _isValid;
        
        protected override Task OnInitializedAsync()
        {
            if (Db.CanWrite == false)
                Snackbar.Add("Нет доступа для записи в базу данных", Severity.Error);
            
            return Task.CompletedTask;
        }
        
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            
            if (IsEditMode)
            {
                if (EditedProduct is null)
                    throw new ArgumentException("EditedProduct is null in edit mode");
                
                Barcode          = EditedProduct.Barcode;
                QuantityPerBox   = EditedProduct.QuantityPerBox;
                QuantityPerShelf = EditedProduct.QuantityPerShelf;
                ProductName      = EditedProduct.Name;
            }
        }
        
        private async Task Submit()
        {
            try
            {
                if (IsEditMode)
                    await Db.UpdateAsync(EditedProduct);
                else
                    await Db.AddAsync(EditedProduct);
            }
            catch (HttpRequestException e)
            {
                Console.Error.WriteLine(e);
                Snackbar.Add("Ошибка при сохранении товара", Severity.Error);
                return;
            }
            
            MudDialog?.Close(EditedProduct);
        }
        
        private void Cancel()
        {
            MudDialog?.Close(DialogResult.Cancel());
        }
        
        private async Task<string> ArticleValidation(string arg)
        {
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