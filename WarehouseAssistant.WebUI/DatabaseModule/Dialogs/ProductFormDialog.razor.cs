using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.DatabaseModule
{
    public partial class ProductFormDialog : ComponentBase
    {
        [Obsolete]
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
        
        [Obsolete]
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
        
        [Obsolete]
        public static async Task<Product?> ShowAddDialogAsync(IDialogService dialogService)
        {
            Product product = new();
            
            bool success = await ShowAddDialogAsync(product, dialogService);
            
            return success ? product : null;
        }
        
        [Obsolete]
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
        
        [Inject] public AuthenticationStateProvider AuthenticationState { get; set; } = null!;
        
        public string? Article
        {
            get => _model.Article;
            set => _model.Article = value;
        }
        
        public string? ProductName
        {
            get => _model.Name;
            set => _model.Name = value;
        }
        
        public string? Barcode
        {
            get => _model.Barcode;
            set => _model.Barcode = value;
        }
        
        public int? QuantityPerBox
        {
            get => _model.QuantityPerBox;
            set => _model.QuantityPerBox = value;
        }
        
        public int? QuantityPerShelf
        {
            get => _model.QuantityPerShelf;
            set => _model.QuantityPerShelf = value;
        }
        
        private bool    _isValid;
        private MudForm _form = null!;
        private bool    _isLoading;
        private Product _model = new();
        
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            var state = await AuthenticationState.GetAuthenticationStateAsync();
            
            if (!state.User.IsInRole("Admin") && !state.User.IsInRole("Editor"))
            {
                Snackbar.Add("Нет прав на добавление/редактирование товаров", Severity.Error);
                MudDialog?.Cancel();
            }
        }
        
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            
            if (EditedProduct is null)
                throw new ArgumentException("EditedProduct is null");
            
            Article          = EditedProduct.Article;
            ProductName      = EditedProduct.Name;
            Barcode          = EditedProduct.Barcode;
            QuantityPerBox   = EditedProduct.QuantityPerBox;
            QuantityPerShelf = EditedProduct.QuantityPerShelf;
            
            Debug.WriteLine($"Set parameters: {Article}, {ProductName}, {Barcode}, {QuantityPerBox}, {QuantityPerShelf}"
                , nameof(ProductFormDialog));
        }
        
        // TODO Add log
        private async Task Submit()
        {
            _isLoading = true;
            
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
            finally
            {
                _isLoading = false;
            }
        }
        
        private void Cancel()
        {
            MudDialog?.Close(DialogResult.Cancel());
        }
        
        internal async Task<string> ArticleValidation(string? arg)
        {
            if (IsEditMode) return null!;
            
            if (string.IsNullOrEmpty(arg)) return "Артикул обязателен";
            
            if (StartsAndEndsWithNonWhitespaceChar(arg) == false) return "Артикул не должен содержать пробелы";
            
            _isLoading = true;
            if (await Db.GetByArticleAsync(arg) != null)
            {
                _isLoading = false;
                return "Товар с данным артикулом существует";
            }
            
            _isLoading = false;
            return null!;
        }
        
        public static bool StartsAndEndsWithNonWhitespaceChar(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            return !char.IsWhiteSpace(input[0]) && !char.IsWhiteSpace(input[^1]);
        }
    }
}