using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Infrastructure.Data.Configurations;

public class TaskUserConfiguration : IEntityTypeConfiguration<TaskUser>
{
    public void Configure(EntityTypeBuilder<TaskUser> builder)
    {
        builder.HasKey(tu => new { tu.UserId, tu.TaskId });

        builder.Property(tu => tu.UserId)
            .HasMaxLength(36);

        builder.Property(tu => tu.TaskId)
            .HasMaxLength(36);

        builder.HasOne(tu => tu.User)
            .WithMany(u => u.TaskUsers)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tu => tu.Task)
            .WithMany(t => t.TaskUsers)
            .HasForeignKey(tu => tu.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
