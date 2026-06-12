using FluentValidation;

namespace Gdc.Modules.Reglas.Application.Secretariat.Validators;

public sealed class CreateReglasSecretariatContactRequestValidator
    : AbstractValidator<CreateReglasSecretariatContactRequest>
{
    public CreateReglasSecretariatContactRequestValidator()
    {
        RuleFor(x => x.SecretariatCode)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.SecretariatName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ContactName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.ContactPhone)
            .MaximumLength(32)
            .When(x => x.ContactPhone is not null);
    }
}
