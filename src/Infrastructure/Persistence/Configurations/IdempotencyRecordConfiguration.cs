using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Key).HasMaxLength(100).IsRequired();
        builder.Property(record => record.RequestHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(record => new { record.UserId, record.Key }).IsUnique();
        builder.HasIndex(record => record.LeaveRequestId)
            .IsUnique()
            .HasFilter("[LeaveRequestId] IS NOT NULL");

        builder.HasOne(record => record.User)
            .WithMany(user => user.IdempotencyRecords)
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(record => record.LeaveRequest)
            .WithOne(request => request.IdempotencyRecord)
            .HasForeignKey<IdempotencyRecord>(record => record.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
