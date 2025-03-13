using System;

namespace BookingsSportsFields.Core.Model
{
    public class Invitations
    {
        /// <summary>
        /// Унікальний ідентифікатор запрошення
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// Email запрошеного друга
        /// </summary>
        public string Email { get; } = string.Empty;
        /// <summary>
        /// ('Pending', 'Accepted', 'Declined'))  -- Статус запрошення
        /// </summary>
        public InvitationsStatus Status { get; }
        /// <summary>
        /// Дата створення запрошення
        /// </summary>
        public DateTime CreatedAt { get; } 
    }
    public enum InvitationsStatus
    {
        Pending = 1,
        Accepted = 2,
        Declined = 3
    }
}
