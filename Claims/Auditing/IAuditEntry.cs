namespace Claims.Auditing
{
    /// <summary>
    /// An audit row that can go on the audit queue. This is an interface rather than a
    /// base class, because EF Core would map a base class as inheritance and change the
    /// migrated schema.
    /// </summary>
    public interface IAuditEntry
    {
    }
}
