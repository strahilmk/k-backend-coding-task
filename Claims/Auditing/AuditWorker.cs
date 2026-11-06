using System.Threading.Channels;

namespace Claims.Auditing
{
    /// <summary>
    /// Reads audit entries from the queue and saves each one to SQL Server, one at a time.
    /// A failed save is logged and not retried. On shutdown it writes every entry
    /// still in the queue before stopping.
    /// </summary>
    public sealed class AuditWorker(
        Channel<IAuditEntry> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // No stoppingToken here: it is cancelled as soon as shutdown starts, which would
            // drop queued entries. StopAsync completes the channel instead, so the loop ends
            // once every queued entry is written.
            await foreach (var entry in channel.Reader.ReadAllAsync())
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
                    context.Add(entry);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to save audit entry");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            channel.Writer.TryComplete();
            await base.StopAsync(cancellationToken);
        }
    }
}
