using BookingsSportsFields.Core.Model;
using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class BookingsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор бронювання
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Коментар до бронювання
        /// </summary>
        public string? Comment { get; set; }
        /// <summary>
        /// Час початку бронювання
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Час закінчення бронювання
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// (Pending', 'Confirmed', 'Cancelled')) -- Статус бронювання
        /// </summary>
        public BookingStatus Status { get; set; }
        /// <summary>
        /// Загальна вартість бронювання
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// Дата бронювання
        /// </summary>
        public DateTime CreatedAt { get; set; } 
        /// <summary>
        /// Ідентифікатор Користувач, який забронював
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Користувач, який забронював
        /// </summary>
        public UserEntity User { get; set; } = null!;
        /// <summary>
        /// Ідентифікатор спотривного майданчику
        /// </summary>
        public Guid SportsFieldId {  get; set; }
        /// <summary>
        /// Спортивний майданчик
        /// </summary>
        public SportsFieldsEntity SportsField { get; set; } = null!; 

        public Guid? ReviewsId { get; set; }

        public ReviewsEntity? Reviews{ get; set; } = null!;
    }
}
