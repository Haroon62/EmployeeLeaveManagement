using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes", table =>
            table.HasCheckConstraint("CK_LeaveTypes_DefaultAnnualDays", "[DefaultAnnualDays] >= 0"));
        builder.HasKey(leaveType => leaveType.Id);

        builder.Property(leaveType => leaveType.Name).HasMaxLength(100).IsRequired();
        builder.Property(leaveType => leaveType.Description).HasMaxLength(500);

        builder.HasIndex(leaveType => leaveType.Name).IsUnique();
    }
}
