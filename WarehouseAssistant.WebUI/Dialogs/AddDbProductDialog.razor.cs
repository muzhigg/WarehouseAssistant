﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using WarehouseAssistant.Core.Models;
using WarehouseAssistant.Data.Models;
using WarehouseAssistant.Data.Repositories;

namespace WarehouseAssistant.WebUI.Dialogs
{
    public partial class AddDbProductDialog : ComponentBase
    {
        public static async Task<Product?> Show(ProductTableItem product, IDialogService dialogService)
        {
            DialogParameters<AddDbProductDialog> parameters = [];
            parameters.Add(productDialog => productDialog.Article, product.Article);
            parameters.Add(productDialog => productDialog.ProductName, product.Name);

            IDialogReference? dialog = await dialogService.ShowAsync<AddDbProductDialog>("Добавить товар", parameters);
            DialogResult?     result = await dialog.Result;

            if (!result.Canceled) return (Product)result.Data;

            return null;
        }

        [Parameter] public string? Article { get; set; }
        [Parameter] public string? ProductName { get; set; }
        public long? Barcode { get; set; }
        public int? QuantityPerBox { get; set; }
        public int? QuantityPerShelf { get; set; }

        [CascadingParameter] private MudDialogInstance? MudDialog { get; set; }
        [Inject]             private IRepository<Product>      Db        { get; set; } = null!;

        private bool _isValid;

        private async Task Submit()
        {
            Product product = new()
            {
                Article = Article,
                Barcode = Barcode,
                Name = ProductName,
                QuantityPerBox = QuantityPerBox,
                QuantityPerShelf = QuantityPerShelf
            };
            // TODO need test to exceptions
            await Db.AddAsync(product);

            MudDialog?.Close(product);
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
