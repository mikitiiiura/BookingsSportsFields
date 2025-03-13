using System;

namespace BookingsSportsFields.Core.Model
{
    public class Reviews
    {
        public Reviews(Guid id, byte rating, string comment, DateTime createdAt)
        {
            Id = id;
            Rating = rating;
            Comment = comment;
            CreatedAt = createdAt;
        }
        public Guid Id { get; } //Унікальний ідентифікатор відгуку
        public byte Rating { get; } //Оцінка від 1 до 5
        public string Comment { get; } = string.Empty; //Текстовий відгук
        public DateTime CreatedAt { get; } //Дата створення відгуку
    }
}
