using BookingsSportsFields.DataAccess.Configurations;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace BookingsSportsFields.DataAccess
{
    public class BookingsSportsFieldsDBContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>//DbContext
    {
        public BookingsSportsFieldsDBContext(DbContextOptions<BookingsSportsFieldsDBContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Userss { get; set; }
        public DbSet<BookingsEntity> Bookings { get; set; }
        public DbSet<DiscountsEntity> Discounts { get; set; }
        public DbSet<InvitationsEntity> Invitations { get; set; }
        public DbSet<LocationsEntity> Locations { get; set; }
        public DbSet<ReviewsEntity> Reviews { get; set; }
        public DbSet<SportsFieldsEntity> SportsFields { get; set; }
        //public DbSet<SportsFieldImageEntity> SportsFieldImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BookingDiscountsConfiguration());
            modelBuilder.ApplyConfiguration(new BookingsConfiguration());
            modelBuilder.ApplyConfiguration(new DiscountsConfiguration());
            modelBuilder.ApplyConfiguration(new InvitationsConfiguration());
            modelBuilder.ApplyConfiguration(new LocationsConfiguration());
            modelBuilder.ApplyConfiguration(new ReviewsConfiguration());
            //modelBuilder.ApplyConfiguration(new SportsFieldImageConfiguration());
            modelBuilder.ApplyConfiguration(new SportsFieldsConfiguration());
            //modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.HasDefaultSchema("identity");
        }
    }
}
