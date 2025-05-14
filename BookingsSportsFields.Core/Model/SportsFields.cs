
using System;

namespace BookingsSportsFields.Core.Model
{
    public class SportsFields
    {

        public SportsFields(Guid id, string name, SportFieldsType type, double price, string description, DateTime createdAt)
        {
            Id = id; 
            Name = name;
            Type = type;
            PricePerHour = price;
            Description = description;
            CreatedAt = createdAt;
        }
        Guid Id { get; } // Унікальний ідентифікатор майданчика
        string Name { get; } = string.Empty;  // Назва майданчика
        //Location // Адреса або GPS-координати
        SportFieldsType Type { get; }  // Тип спорту(футбол, теніс, баскетбол тощо)
        double PricePerHour { get; }  // Вартість оренди за годину
        //string[] ImageUrl;  // Посилання на зображення(масив)
        string Description { get; } = string.Empty; // Опис майданчика
        //OwnerId  // Власник(якщо є)
        DateTime CreatedAt { get; } // Дата додавання
    }

    public enum SportFieldsType
    {
        Tennis = 0,
        Badminton = 1,
        Football = 2,
        Box = 3,
        PingPong = 4,
        Biliard = 5,
        Basketball = 6
    }
}
