using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class DiscountsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор знижки
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Промокод
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// Відсоток знижки
        /// </summary>
        public byte DiscountPercentage { get; set; }
        /// <summary>
        /// Дата закінчення дії знижки
        /// </summary>
        public DateTime ExpiryDate { get; set; }
        /// <summary>
        /// Чи активна знижка
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Дата створення знижки
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
