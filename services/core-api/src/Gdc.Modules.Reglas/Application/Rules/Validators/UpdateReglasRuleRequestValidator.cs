using FluentValidation;

namespace Gdc.Modules.Reglas.Application.Rules.Validators;

public sealed class UpdateReglasRuleRequestValidator : AbstractValidator<UpdateReglasRuleRequest>
{
    public UpdateReglasRuleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(512)
            .When(x => x.Description is not null);

        RuleFor(x => x.EmailSubject)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.EmailBodyHtml)
            .NotEmpty();

        When(x => x.IsActive, () =>
        {
            RuleFor(x => x.PdfTemplateId)
                .NotEqual(Guid.Empty)
                .WithMessage("An active rule must reference a PDF template.");

            RuleFor(x => x.ConditionRoot)
                .NotNull()
                .WithMessage("An active rule must define at least one condition.");

            RuleFor(x => x.ConditionRoot!)
                .SetValidator(new ConditionNodeValidator())
                .When(x => x.ConditionRoot is not null);

            RuleFor(x => x.ConditionRoot)
                .Must(root => ReglasConditionTreeMapper.CountPredicates(root) > 0)
                .WithMessage("An active rule must include at least one predicate condition.");
        });

        When(x => x.ConditionRoot is not null, () =>
        {
            RuleFor(x => x.ConditionRoot!)
                .SetValidator(new ConditionNodeValidator());
        });
    }
}
