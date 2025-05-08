using BookingsSportsFields.Core.Model;
using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class SportsFieldsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор майданчика
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Назва майданчика
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Певна важлива інформація для користувачів
        /// </summary>
        public string? WarningInformation {  get; set; }
        /// <summary>
        /// Тип спорту(футбол, теніс, баскетбол тощо)
        /// </summary>
        public SportFieldsType Type { get; set; }
        /// <summary>
        /// Вартість оренди за годину
        /// </summary>
        public double PricePerHour { get; set; }
        /// <summary>
        /// Опис майданчика
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Дата додавання
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// ID Адреса або GPS-координати
        /// </summary>
        public Guid LocationId { get; set; }
        /// <summary>
        /// Адреса або GPS-координати
        /// </summary>
        public LocationsEntity Location { get; set; } = null!;
        /// <summary>
        /// ID Власника(якщо є)
        /// </summary>
        ///////////////// public Guid? OwnerId { get; set; }
        public Guid? OwnerId { get; set; }
        /// <summary>
        /// Власник(якщо є)
        /// </summary>
        public UserEntity? Owner { get; set; } 
        /// <summary>
        /// зображення майданчика
        /// </summary>
        public string ImageUrl { get; set; } = null!;

        public List<BookingsEntity> Bookings { get; set; } = [];
    }
}
