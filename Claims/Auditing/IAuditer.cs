namespace Claims.Auditing
{
    /// <summary>
    /// Queues an audit entry for each create and delete of a claim or cover.
    /// <see cref="AuditWorker"/> writes the entries to SQL Server in the background,
    /// so these methods return without waiting for the database.
    /// </summary>
    public interface IAuditer
    {
        void AuditClaim(string id, string httpRequestType);
        void AuditCover(string id, string httpRequestType);
    }
}
