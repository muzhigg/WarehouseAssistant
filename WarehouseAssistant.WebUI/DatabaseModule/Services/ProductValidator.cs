using FluentValidation;
using WarehouseAssistant.Data.Repositories;
using WarehouseAssistant.Shared.Models.Db;

namespace WarehouseAssistant.WebUI.DatabaseModule;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator(IRepository<Product> repository)
    {
        RuleSet("Add", () =>
        {
            RuleFor(p => p.Article)
                .NotNull().WithMessage("Артикул обязателен")
                .NotEmpty().WithMessage("Артикул обязателен")
                .MustAsync(async (s, token) =>
                    await repository.ContainsAsync(s) == false).WithMessage("Такой артикул уже существует");
        });
        
        RuleSet("Edit", () =>
        {
            RuleFor(p => p.Article)
                .NotNull().WithMessage("Артикул обязателен")
                .NotEmpty().WithMessage("Артикул обязателен");
        });
        
        RuleFor(p => p.Name)
            .NotNull().WithMessage("Название обязательно")
            .NotEmpty().WithMessage("Название обязательно");
        
        RuleFor(p => p.QuantityPerBox)
            .NotEqual(0).WithMessage("Количество не может быть равно 0")
            .Unless(p => p.QuantityPerBox == null);
        
        RuleFor(p => p.QuantityPerShelf)
            .NotEqual(0).WithMessage("Количество не может быть равно 0")
            .Unless(p => p.QuantityPerShelf == null);
    }
}