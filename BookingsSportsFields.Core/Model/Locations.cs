using System;
using System.Data;

namespace BookingsSportsFields.Core.Model
{
    public class Locations
    {
        public Locations(Guid id, decimal latitude, decimal longitude, string address, DateTime createdAt)
        {
            Id = id;
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
            CreatedAt = createdAt;
        }
        public Guid Id { get; } //Унікальний ідентифікатор локації
        public decimal Latitude { get; } //Географічна широта
        public decimal Longitude { get; } //Географічна довгота
        public string Address { get; } = string.Empty;//Повна адреса
        public DateTime CreatedAt { get; } //Дата додавання

        //SportsFieldId  -- ID майданчика
    }
}
