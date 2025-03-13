using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class ReviewsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор відгуку
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Оцінка від 1 до 5
        /// </summary>
        public byte Rating { get; set; }
        /// <summary>
        /// Текстовий відгук
        /// </summary>
        public string Comment { get; set; } = string.Empty;
        /// <summary>
        /// Дата створення відгуку
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// ID користувача який залишив відгук або залишитись анонімом
        /// </summary>
        ////////////////public Guid? UserId { get; set; }
        public Guid? UserId { get; set; }
        /// <summary>
        /// Користувач який залишив відгук або залишитись анонімом
        /// </summary>
        public UserEntity? User { get; set; }
        /// <summary>
        /// ID Майданчика, до якого відгу
        /// </summary>
        public Guid SportsFieldId { get; set; }
        /// <summary>
        /// Майданчик, до якого відгу
        /// </summary>
        public SportsFieldsEntity SportsField { get; set; } = null!;
    }
}
