using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances", table =>
        {
            table.HasCheckConstraint("CK_LeaveBalances_AllocatedDays", "[AllocatedDays] >= 0");
            table.HasCheckConstraint("CK_LeaveBalances_UsedDays", "[UsedDays] >= 0");
            table.HasCheckConstraint("CK_LeaveBalances_RemainingDays", "[RemainingDays] >= 0");
            table.HasCheckConstraint(
                "CK_LeaveBalances_TotalDays",
                "[AllocatedDays] = [UsedDays] + [RemainingDays]");
            table.HasCheckConstraint("CK_LeaveBalances_Year", "[Year] >= 2000 AND [Year] <= 2100");
        });

        builder.HasKey(balance => balance.Id);
        builder.Property(balance => balance.RowVersion).IsRowVersion();

        builder.HasIndex(balance => new { balance.EmployeeId, balance.LeaveTypeId, balance.Year })
            .IsUnique();

        builder.HasOne(balance => balance.Employee)
            .WithMany(employee => employee.LeaveBalances)
            .HasForeignKey(balance => balance.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(balance => balance.LeaveType)
            .WithMany(leaveType => leaveType.LeaveBalances)
            .HasForeignKey(balance => balance.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
