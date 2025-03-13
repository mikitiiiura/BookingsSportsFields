using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class BookingDiscountsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Сума знижки у валюті
        /// </summary>
        public decimal DiscountAmount { get; set; }
        /// <summary>
        /// ID бронювання
        /// </summary>
        public Guid BookingId { get; set; }
        /// <summary>
        /// Бронювання
        /// </summary>
        public BookingsEntity BookingName { get; set; } = null!;
        /// <summary>
        /// ID знижки
        /// </summary>
        public Guid DiscountId { get; set; }
        /// <summary>
        /// Знижка
        /// </summary>
        public DiscountsEntity Discount { get; set; } = null!;
    }
}
