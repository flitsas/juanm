using FluentValidation;

namespace Gdc.Modules.Reglas.Application.Rules.Validators;

internal sealed class ConditionNodeValidator : AbstractValidator<ConditionNodeDto>
{
    public ConditionNodeValidator()
    {
        RuleFor(x => x.NodeType)
            .NotEmpty()
            .Must(BeSupportedNodeType)
            .WithMessage("Unsupported node type.");

        When(x => ReglasNodeTypes.Group.Equals(x.NodeType, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.LogicOperator)
                .NotEmpty()
                .Must(BeSupportedLogicOperator)
                .WithMessage("Group nodes require logicOperator and or.");

            RuleFor(x => x.Children)
                .NotNull()
                .Must(children => children is { Count: > 0 })
                .WithMessage("Group nodes require at least one child.");

            RuleForEach(x => x.Children!)
                .SetValidator(this);
        });

        When(x => ReglasNodeTypes.Predicate.Equals(x.NodeType, StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.FieldKey)
                .NotEmpty()
                .Must(key => key is not null && ReglasFieldKeys.All.Contains(key))
                .WithMessage("Unsupported comparendo field key.");

            RuleFor(x => x.ComparisonOperator)
                .NotEmpty()
                .Must(op => op is not null && ReglasComparisonOperatorSet.All.Contains(op))
                .WithMessage("Unsupported comparison operator.");

            RuleFor(x => x.ComparisonValue)
                .NotEmpty()
                .WithMessage("Predicate nodes require comparisonValue.");

            RuleFor(x => x.Children)
                .Must(children => children is null or { Count: 0 })
                .WithMessage("Predicate nodes cannot have children.");
        });
    }

    private static bool BeSupportedNodeType(string nodeType) =>
        ReglasNodeTypes.Group.Equals(nodeType, StringComparison.OrdinalIgnoreCase)
        || ReglasNodeTypes.Predicate.Equals(nodeType, StringComparison.OrdinalIgnoreCase);

    private static bool BeSupportedLogicOperator(string? logicOperator) =>
        ReglasLogicOperators.And.Equals(logicOperator, StringComparison.OrdinalIgnoreCase)
        || ReglasLogicOperators.Or.Equals(logicOperator, StringComparison.OrdinalIgnoreCase);
}
