using LabLedger.DataModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabLedger.DataModel.Configurations;

public class SampleConfiguration : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Origin)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne(s => s.SubmittedBy)
            .WithMany()
            .HasForeignKey(s => s.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Tests)
            .WithOne(t => t.Sample)
            .HasForeignKey(t => t.SampleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}