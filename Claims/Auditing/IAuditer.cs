namespace Claims.Auditing
{
    /// <summary>
    /// Records each create and delete of a claim or cover as a row in SQL Server.
    /// </summary>
    public interface IAuditer
    {
        void AuditClaim(string id, string httpRequestType);
        void AuditCover(string id, string httpRequestType);
    }
}
