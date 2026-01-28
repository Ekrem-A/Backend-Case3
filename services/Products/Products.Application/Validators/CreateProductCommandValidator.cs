using FluentValidation;
using Products.Application.Commands;

namespace Products.Application.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması zorunludur")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU zorunludur")
            .MaximumLength(50).WithMessage("SKU en fazla 50 karakter olabilir")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("SKU sadece harf, rakam ve tire içerebilir");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka zorunludur")
            .MaximumLength(100).WithMessage("Marka en fazla 100 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok 0 veya daha büyük olmalıdır");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur");
    }
}

