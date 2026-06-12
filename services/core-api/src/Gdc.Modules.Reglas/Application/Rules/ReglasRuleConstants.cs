namespace Gdc.Modules.Reglas.Application.Rules;

public static class ReglasNodeTypes
{
    public const string Group = "group";

    public const string Predicate = "predicate";
}

public static class ReglasLogicOperators
{
    public const string And = "and";

    public const string Or = "or";
}

public static class ReglasComparisonOperators
{
    public const string Equal = "eq";

    public const string NotEqual = "neq";

    public const string GreaterThan = "gt";

    public const string LessThan = "lt";

    public const string Contains = "contains";

    public const string NotContains = "not_contains";
}

public static class ReglasFieldKeys
{
    public const string Estado = "estado";

    public const string NumeroComparendo = "numero_comparendo";

    public const string Placa = "placa";

    public const string InfractorNombre = "infractor_nombre";

    public const string FechaComparendo = "fecha_comparendo";

    public const string FechaNotificacion = "fecha_notificacion";

    public const string DestinoEmail = "destino_email";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Estado,
        NumeroComparendo,
        Placa,
        InfractorNombre,
        FechaComparendo,
        FechaNotificacion,
        DestinoEmail,
    };
}

public static class ReglasComparisonOperatorSet
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ReglasComparisonOperators.Equal,
        ReglasComparisonOperators.NotEqual,
        ReglasComparisonOperators.GreaterThan,
        ReglasComparisonOperators.LessThan,
        ReglasComparisonOperators.Contains,
        ReglasComparisonOperators.NotContains,
    };
}
