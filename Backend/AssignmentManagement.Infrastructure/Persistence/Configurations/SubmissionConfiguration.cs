using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentManagement.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.AnswerText)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.SubmittedAtUtc)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.MarksObtained);

        builder.Property(s => s.Feedback)
            .HasMaxLength(2000);

        // One submission per student per assignment.
        builder.HasIndex(s => new { s.AssignmentId, s.StudentId })
            .IsUnique();

        builder.HasOne(s => s.Assignment)
            .WithMany()
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Student)
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}