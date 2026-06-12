namespace Gdc.Modules.Reglas.Application.Execution;

public sealed record ReglasProcessResponse(
    int ProcessedCount,
    int SucceededCount,
    int FailedCount);
