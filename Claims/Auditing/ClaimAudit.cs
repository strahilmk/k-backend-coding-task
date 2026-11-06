namespace Claims.Auditing
{
    public class ClaimAudit : IAuditEntry
    {
        public int Id { get; set; }

        public required string ClaimId { get; set; }

        public DateTime Created { get; set; }

        public required HttpRequestType HttpRequestType { get; set; }
    }
}
