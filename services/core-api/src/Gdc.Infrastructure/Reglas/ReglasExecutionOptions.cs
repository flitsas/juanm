namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasExecutionOptions
{
    public const string SectionName = "Reglas:Execution";

    public bool Enabled { get; set; }

    public int PollIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 50;

    public bool ProcessMatchesAfterEvaluation { get; set; } = true;
}
