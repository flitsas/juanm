namespace Gdc.Modules.Reglas.Application.Execution;

public static class ReglasRunStatuses
{
    public const string Running = "running";

    public const string Completed = "completed";

    public const string Failed = "failed";
}

public static class ReglasTriggerTypes
{
    public const string Scheduled = "scheduled";

    public const string Manual = "manual";
}

public static class ReglasProcessingStatuses
{
    public const string Matched = "matched";

    public const string Success = "success";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
