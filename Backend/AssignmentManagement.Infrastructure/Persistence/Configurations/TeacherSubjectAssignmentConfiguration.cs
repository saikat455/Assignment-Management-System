using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Persistence.Configurations;

public class TeacherSubjectAssignmentConfiguration : IEntityTypeConfiguration<TeacherSubjectAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherSubjectAssignment> builder)
    {
        builder.ToTable("TeacherSubjectAssignments");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new { t.TeacherId, t.SubjectId })
            .IsUnique();

        builder.HasOne(t => t.Teacher)
            .WithMany()
            .HasForeignKey(t => t.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Subject)
            .WithMany(s => s.TeacherAssignments)
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}