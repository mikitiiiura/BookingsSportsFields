using BookingsSportsFields.Core.Model;
using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class InvitationsEntity
    {
        /// <summary>
        /// Унікальний ідентифікатор запрошення
        /// </summary>
        public Guid Id { get; set; } 
        /// <summary>
        /// Email запрошеного друга
        /// </summary>
        public string Email { get; set; } = string.Empty; 
        /// <summary>
        /// ('Pending', 'Accepted', 'Declined') -- Статус запрошення
        /// </summary>
        public InvitationsStatus Status { get; set; } 
        /// <summary>
        /// Дата створення запрошення
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// ID бронювання
        /// </summary>
        public Guid BookingId { get; set; }
        /// <summary>
        /// Бронювання
        /// </summary>
        public BookingsEntity? Bookings { get; set; }

        /// <summary>
        /// ID користувача, який запросив
        /// </summary>
        /////////////////////public Guid? InviterId { get; set; }
        public Guid? InviterId { get; set; }
        /// <summary>
        /// Користувач, який запросив
        /// </summary>
        public UserEntity? InviterUser { get; set; }
    }
}
