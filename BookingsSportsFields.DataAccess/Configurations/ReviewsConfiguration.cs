using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingsSportsFields.DataAccess.ModelEntity;

public class ReviewsConfiguration : IEntityTypeConfiguration<ReviewsEntity>
{
    public void Configure(EntityTypeBuilder<ReviewsEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.SportsField)
            .WithMany()
            .HasForeignKey(r => r.SportsFieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
