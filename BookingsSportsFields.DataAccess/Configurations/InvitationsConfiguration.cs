using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class InvitationsConfiguration : IEntityTypeConfiguration<InvitationsEntity>
{
    public void Configure(EntityTypeBuilder<InvitationsEntity> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(100);

        // Зовнішній ключ на Bookings
        builder.HasOne(i => i.Bookings)
            .WithMany()
            .HasForeignKey(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade); // Каскадне видалення для BookingId

        // Зовнішній ключ на Users (InviterUser)
        builder.HasOne(i => i.InviterUser)
            .WithMany()
            .HasForeignKey(i => i.InviterId)
            .OnDelete(DeleteBehavior.SetNull); // Змінюємо на NoAction
    }
}