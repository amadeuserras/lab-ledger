using LabLedger.DataModel.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabLedger.DataModel.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasIndex(r => r.TestId)
            .IsUnique();

        builder.HasOne(r => r.RecordedBy)
            .WithMany()
            .HasForeignKey(r => r.RecordedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PublishedBy)
            .WithMany()
            .HasForeignKey(r => r.PublishedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
