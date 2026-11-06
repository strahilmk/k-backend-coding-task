using System.Threading.Channels;

namespace Claims.Auditing
{
    /// <summary>
    /// Writes audit entries to the in-memory queue that <see cref="AuditWorker"/> reads.
    /// If the queue is full or closed, the entry is dropped and logged, so a request
    /// never waits for auditing.
    /// </summary>
    public class Auditer : IAuditer
    {
        private readonly Channel<IAuditEntry> _channel;
        private readonly ILogger<Auditer> _logger;

        public Auditer(Channel<IAuditEntry> channel, ILogger<Auditer> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        public void AuditClaim(string id, string httpRequestType)
        {
            Enqueue(new ClaimAudit
            {
                Created = DateTime.UtcNow,
                HttpRequestType = httpRequestType,
                ClaimId = id
            }, id);
        }

        public void AuditCover(string id, string httpRequestType)
        {
            Enqueue(new CoverAudit
            {
                Created = DateTime.UtcNow,
                HttpRequestType = httpRequestType,
                CoverId = id
            }, id);
        }

        private void Enqueue(IAuditEntry entry, string id)
        {
            if (!_channel.Writer.TryWrite(entry))
            {
                _logger.LogError("Audit queue is full or closed, dropped entry for {Id}", id);
            }
        }
    }
}