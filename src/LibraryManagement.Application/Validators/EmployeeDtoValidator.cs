using FluentValidation;
using LibraryManagement.Application.Dtos;

namespace LibraryManagement.Application.Validators;

public class EmployeeDtoValidator : AbstractValidator<EmployeeDto>
{
    public EmployeeDtoValidator()
    {
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия сотрудника обязательна.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя сотрудника обязательно.")
            .MaximumLength(100);

        RuleFor(x => x.MiddleName)
            .MaximumLength(100);

        RuleFor(x => x.PositionId)
            .GreaterThan(0).WithMessage("Выберите должность.");

        RuleFor(x => x.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Некорректный формат email.")
            .MaximumLength(150);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow)
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Дата рождения не может быть в будущем.");
    }
}
