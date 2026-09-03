using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests", table =>
        {
            table.HasCheckConstraint("CK_LeaveRequests_DateRange", "[EndDate] >= [StartDate]");
            table.HasCheckConstraint("CK_LeaveRequests_WorkingDays", "[WorkingDays] > 0");
            table.HasCheckConstraint(
                "CK_LeaveRequests_RejectionReason",
                "[Status] <> 'Rejected' OR LEN(LTRIM(RTRIM([RejectionReason]))) > 0");
        });

        builder.HasKey(request => request.Id);
        builder.Property(request => request.StartDate).HasColumnType("date");
        builder.Property(request => request.EndDate).HasColumnType("date");
        builder.Property(request => request.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(request => request.RejectionReason).HasMaxLength(1000);
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(request => request.RowVersion).IsRowVersion();

        builder.HasIndex(request => new { request.EmployeeId, request.StartDate, request.EndDate });
        builder.HasIndex(request => new { request.EmployeeId, request.Status });

        builder.HasOne(request => request.Employee)
            .WithMany(employee => employee.LeaveRequests)
            .HasForeignKey(request => request.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.LeaveType)
            .WithMany(leaveType => leaveType.LeaveRequests)
            .HasForeignKey(request => request.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.ReviewedByEmployee)
            .WithMany(employee => employee.ReviewedLeaveRequests)
            .HasForeignKey(request => request.ReviewedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
