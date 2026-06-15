using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class PositionDtoValidator : AbstractValidator<PositionDto>
{
    public PositionDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название должности обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
