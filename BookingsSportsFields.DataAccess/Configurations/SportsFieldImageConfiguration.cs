//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using BookingsSportsFields.DataAccess.ModelEntity;

//public class SportsFieldImageConfiguration : IEntityTypeConfiguration<SportsFieldImageEntity>
//{
//    public void Configure(EntityTypeBuilder<SportsFieldImageEntity> builder)
//    {
//        builder.HasKey(i => i.Id);

//        builder.Property(i => i.ImageUrl)
//            .IsRequired();

//        builder.HasOne(i => i.SportsField)
//            .WithMany(s => s.Images)
//            .HasForeignKey(i => i.SportsFieldId)
//            .OnDelete(DeleteBehavior.Cascade);
//    }
//}
