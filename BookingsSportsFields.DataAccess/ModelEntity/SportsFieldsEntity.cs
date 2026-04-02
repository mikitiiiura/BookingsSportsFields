using BookingsSportsFields.Core.Model;
using System;
using System.Text.Json.Serialization;

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
        // /// <summary>
        // /// Певна важлива інформація для користувачів
        // /// </summary>
        // public string? WarningInformation {  get; set; }
        // /// <summary>
        // /// Тип спорту(футбол, теніс, баскетбол тощо)
        // /// </summary>
        // public SportFieldsType Type { get; set; }
        // /// <summary>
        // /// Вартість оренди за годину
        // /// </summary>
        // public double PricePerHour { get; set; }
        /// <summary>
        /// Опис майданчика
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Дата додавання
        /// </summary>
        public DateTime CreatedAt { get; set; }

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
        [JsonIgnore]
        public List<BookingsEntity> Bookings { get; set; } = [];
        
        public List<SportsFieldSportTypeEntity> TypesWithDetails  { get; set; } = [];
        
        public bool IsDeleted { get; set; } = false;
        
        public ICollection<SportsFieldInstanceEntity> Instances { get; set; } 
            = new List<SportsFieldInstanceEntity>();
        
        // === НОВЕ ===
        public double AverageRating { get; set; } = 0;           // середній рейтинг (0-5)
        public int ReviewCount { get; set; } = 0;                // кількість відгуків

        public List<ReviewsEntity> Reviews { get; set; } = new(); // для зручності
    }
}
