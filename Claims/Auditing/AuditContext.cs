using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Claims.Auditing
{
    public class AuditContext : DbContext
    {
        // Stores HttpRequestType as "POST" or "DELETE", the values the column has always held.
        // Reading ignores case, and an unknown value throws.
        private static readonly ValueConverter<HttpRequestType, string> HttpRequestTypeConverter = new(
            type => type.ToString().ToUpperInvariant(),
            value => Enum.Parse<HttpRequestType>(value, true));

        public AuditContext(DbContextOptions<AuditContext> options) : base(options)
        {
        }
        public DbSet<ClaimAudit> ClaimAudits { get; set; }
        public DbSet<CoverAudit> CoverAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClaimAudit>().Property(audit => audit.HttpRequestType).HasConversion(HttpRequestTypeConverter);
            modelBuilder.Entity<CoverAudit>().Property(audit => audit.HttpRequestType).HasConversion(HttpRequestTypeConverter);
        }
    }
}
