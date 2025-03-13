using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class SportsFieldImageEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор зображення
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID майданчика
        /// </summary>
        public Guid SportsFieldId { get; set; }

        /// <summary>
        /// URL зображення
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Дата додавання
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Навігаційна властивість до SportsField
        /// </summary>
        public SportsFieldsEntity SportsField { get; set; } = null!;
    }
}
