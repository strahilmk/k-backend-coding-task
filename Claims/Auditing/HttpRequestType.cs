namespace Claims.Auditing
{
    /// <summary>
    /// The request that changed a claim or cover. Stored upper-case ("POST", "DELETE")
    /// in SQL Server, see <see cref="AuditContext"/>.
    /// </summary>
    public enum HttpRequestType
    {
        Post,
        Delete
    }
}
