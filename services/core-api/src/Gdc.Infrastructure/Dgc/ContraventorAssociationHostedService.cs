using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Dgc;

public sealed class DgcContraventorJobOptions
{
    public const string SectionName = "Dgc:ContraventorJob";

    public bool Enabled { get; set; } = true;

    public int PollIntervalMinutes { get; set; } = 1;
}

public sealed class ContraventorAssociationHostedService(
    IServiceScopeFactory scopeFactory,
    ContraventorWindowEvaluator windowEvaluator,
    TimeProvider timeProvider,
    IOptions<DgcContraventorJobOptions> options,
    ILogger<ContraventorAssociationHostedService> logger) : BackgroundService
{
    private readonly HashSet<string> _executedKeys = new(StringComparer.Ordinal);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Contraventor association job is disabled.");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.PollIntervalMinutes));
        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunDueWindowsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Contraventor association window execution failed.");
            }
        }
    }

    private async Task RunDueWindowsAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        PruneExecutionKeys(now);

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GdcDbContext>();
        var job = scope.ServiceProvider.GetRequiredService<ContraventorAssociationJob>();

        var configs = await db.ContraventorJobConfigs
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var config in configs)
        {
            if (!windowEvaluator.IsWindowActive(config.CronExpression, now))
            {
                continue;
            }

            var key = $"{config.TenantId}:{config.WindowOrder}:{now:yyyy-MM-dd-HH-mm}";
            if (!_executedKeys.Add(key))
            {
                continue;
            }

            var processed = await job.ProcessTenantAsync(config.TenantId, cancellationToken);
            logger.LogInformation(
                "Contraventor job window {WindowOrder} tenant {TenantId} processed {Count} comparendos.",
                config.WindowOrder,
                config.TenantId,
                processed);
        }
    }

    private void PruneExecutionKeys(DateTimeOffset now)
    {
        var prefix = now.ToString("yyyy-MM-dd");
        _executedKeys.RemoveWhere(k => !k.Contains(prefix, StringComparison.Ordinal));
    }
}
