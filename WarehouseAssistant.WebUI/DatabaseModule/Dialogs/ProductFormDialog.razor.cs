using System.Diagnostics.CodeAnalysis;
using Blazored.FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Supabase.Postgrest.Exceptions;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models;
using WarehouseAssistant.Shared.Models.Db;
using Severity = MudBlazor.Severity;

namespace WarehouseAssistant.WebUI.DatabaseModule;

public partial class ProductFormDialog : ComponentBase, IDisposable
{
    [Obsolete, ExcludeFromCodeCoverage]
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
    
    [Obsolete, ExcludeFromCodeCoverage]
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
    
    [Obsolete, ExcludeFromCodeCoverage]
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
    
    [Inject]    private IRepository<Product>       Db         { get; set; } = null!;
    [Inject]    private ISnackbar                  Snackbar   { get; set; } = null!;
    [Inject]    private ILogger<ProductFormDialog> Logger     { get; set; } = null!;
    [Parameter] public  bool                       IsEditMode { get; set; }
    
    [Parameter, EditorRequired] public Product EditedProduct { get; set; } = null!;
    
    [CascadingParameter] private MudDialogInstance? MudDialog { get; set; }
    
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
    
    private bool                      _isLoading;
    private EditContext?              _editContext;
    private Product                   _model                   = new();
    private FluentValidationValidator _validator               = null!;
    private CancellationTokenSource   _cancellationTokenSource = new();
    
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
    }
    
    protected override void OnParametersSet()
    {
        if (EditedProduct is null)
        {
            Logger.LogError("EditedProduct is null");
            throw new NullReferenceException("EditedProduct is null");
        }
        
        Logger.LogInformation("Parameters set, copying edited product to local model");
        
        Article          = EditedProduct.Article;
        ProductName      = EditedProduct.Name;
        Barcode          = EditedProduct.Barcode;
        QuantityPerBox   = EditedProduct.QuantityPerBox;
        QuantityPerShelf = EditedProduct.QuantityPerShelf;
    }
    
    private async Task Submit()
    {
        Logger.LogInformation("Submit button clicked");
        
        if (_isLoading || !await _validator.ValidateAsync())
        {
            Logger.LogInformation("Validation failed, returning");
            return;
        }
        
        _isLoading = true;
        
        UpdateEditedProduct();
        
        try
        {
            Logger.LogInformation("Starting DB operation");
            await (IsEditMode
                ? Db.UpdateAsync(EditedProduct, _cancellationTokenSource.Token)
                : Db.AddAsync(EditedProduct, _cancellationTokenSource.Token));
            Logger.LogInformation("DB operation completed");
            
            Snackbar.Add($"Товар {(IsEditMode ? "обновлен" : "добавлен")}" +
                         $"\n{EditedProduct.Article}" +
                         $"\n{EditedProduct.Name}", Severity.Success);
            MudDialog?.Close(true);
        }
        catch (PostgrestException e)
        {
            Logger.LogError(e, "PostgrestException occurred");
            HandleError($"Ошибка при обращении к БД {e.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unknown exception");
            HandleError($"Неизвестная ошибка: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            Logger.LogInformation("Submit method finished");
        }
    }
    
    private void HandleError(string message)
    {
        Snackbar.Add(message, Severity.Error);
        MudDialog?.Close(false);
    }
    
    private void UpdateEditedProduct()
    {
        Logger.LogInformation("Updating EditedProduct properties from form fields");
        
        EditedProduct.Article = Article;
        Logger.LogInformation("Updated Article: {Article}", Article);
        
        EditedProduct.Name = ProductName;
        Logger.LogInformation("Updated Name: {ProductName}", ProductName);
        
        EditedProduct.Barcode = Barcode;
        Logger.LogInformation("Updated Barcode: {Barcode}", Barcode);
        
        EditedProduct.QuantityPerBox = QuantityPerBox;
        Logger.LogInformation("Updated QuantityPerBox: {QuantityPerBox}", QuantityPerBox);
        
        EditedProduct.QuantityPerShelf = QuantityPerShelf;
        Logger.LogInformation("Updated QuantityPerShelf: {QuantityPerShelf}", QuantityPerShelf);
    }
    
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        MudDialog?.Close(DialogResult.Cancel());
    }
    
    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }
}