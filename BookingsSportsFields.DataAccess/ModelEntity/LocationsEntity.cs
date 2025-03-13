using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class LocationsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор локації
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Географічна широта
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Географічна довгота
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Повна адреса
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Дата додавання
        /// </summary>
        public DateTime CreatedAt { get; set; } 
        /// <summary>
        /// Ідентифікатор спотртивного майданчика
        /// </summary>
        public Guid SportsFieldId { get; set; }
        /// <summary>
        /// Спотртивний майданчик
        /// </summary>
        public SportsFieldsEntity SportsFields { get; set; } = null!;
    }
}
