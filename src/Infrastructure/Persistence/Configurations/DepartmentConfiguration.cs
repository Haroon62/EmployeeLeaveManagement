using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(department => department.Id);

        builder.Property(department => department.Name).HasMaxLength(100).IsRequired();
        builder.Property(department => department.Description).HasMaxLength(500);

        builder.HasIndex(department => department.Name).IsUnique();
        builder.HasIndex(department => department.ManagerEmployeeId)
            .IsUnique()
            .HasFilter("[ManagerEmployeeId] IS NOT NULL");

        builder.HasOne(department => department.ManagerEmployee)
            .WithMany(employee => employee.ManagedDepartments)
            .HasForeignKey(department => department.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
