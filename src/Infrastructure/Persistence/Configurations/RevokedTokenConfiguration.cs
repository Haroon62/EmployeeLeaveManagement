using EmployeeLeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeLeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.JwtId).HasMaxLength(100).IsRequired();

        builder.HasIndex(token => token.JwtId).IsUnique();
        builder.HasIndex(token => token.ExpiresAtUtc);

        builder.HasOne(token => token.User)
            .WithMany(user => user.RevokedTokens)
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
