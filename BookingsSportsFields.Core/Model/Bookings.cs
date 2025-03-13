using System;

namespace BookingsSportsFields.Core.Model
{
    public class Bookings
    {
        public Bookings(Guid id, DateTime start, DateTime end, BookingStatus status, decimal totalPrice, DateTime createdAt)
        {
            Id = id;
            StartTime = start;
            EndTime = end;
            Status = status;
            TotalPrice = totalPrice;
            CreatedAt = createdAt;
        }
        public Guid Id { get; } // Унікальний ідентифікатор бронювання
        public DateTime StartTime { get; } //Час початку бронювання
        public DateTime EndTime { get; }  //Час закінчення бронювання
        public BookingStatus Status { get; } //(Pending', 'Confirmed', 'Cancelled')) -- Статус бронювання
        public decimal TotalPrice { get; } // Загальна вартість бронювання
        public DateTime CreatedAt { get; } // Дата бронювання
    }

    public enum BookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
    }
}
