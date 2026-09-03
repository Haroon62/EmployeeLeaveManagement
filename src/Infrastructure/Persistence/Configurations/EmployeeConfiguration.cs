using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.EmployeeNumber).HasMaxLength(30).IsRequired();
        builder.Property(employee => employee.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(employee => employee.LastName).HasMaxLength(100).IsRequired();

        builder.HasIndex(employee => employee.UserId).IsUnique();
        builder.HasIndex(employee => employee.EmployeeNumber).IsUnique();
        builder.HasIndex(employee => employee.DepartmentId);

        builder.HasOne(employee => employee.User)
            .WithOne(user => user.Employee)
            .HasForeignKey<Employee>(employee => employee.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(employee => employee.Department)
            .WithMany(department => department.Employees)
            .HasForeignKey(employee => employee.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(employee => employee.Manager)
            .WithMany(manager => manager.DirectReports)
            .HasForeignKey(employee => employee.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
