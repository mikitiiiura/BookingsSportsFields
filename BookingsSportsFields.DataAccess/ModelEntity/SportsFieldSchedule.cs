using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class SportsFieldSchedule
    {
        public Guid Id { get; set; }

        public Guid SportsFieldSportTypeId { get; set; }
        public SportsFieldSportTypeEntity SportTypeDetail { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan AvailableFrom { get; set; } = new TimeSpan(8, 0, 0); // 08:00 за замовчуванням
        public TimeSpan AvailableTo { get; set; } = new TimeSpan(22, 0, 0);  // 22:00 за замовчуванням
    }
}