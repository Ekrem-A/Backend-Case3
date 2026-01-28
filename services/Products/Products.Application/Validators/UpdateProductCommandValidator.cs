using FluentValidation;
using Products.Application.Commands;

namespace Products.Application.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ürün ID zorunludur");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması zorunludur")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka zorunludur")
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur");
    }
}

