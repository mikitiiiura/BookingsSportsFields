using System;
using System.Collections.Generic;
using BookingsSportsFields.Core.Model;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class SportsFieldSportTypeEntity
    {
        public Guid Id { get; set; }

        public Guid SportsFieldId { get; set; }
        public SportsFieldsEntity SportsField { get; set; } = null!;

        public SportFieldsType Type { get; set; }

        public double PricePerHour { get; set; }

        public string? WarningInformation { get; set; }

        // Розклад по днях тижня для цього виду спорту на майданчику
        public List<SportsFieldSchedule> WeeklySchedules { get; set; } = new();
        
        public ICollection<SportsFieldInstanceEntity> Instances { get; set; } 
            = new List<SportsFieldInstanceEntity>();
    }
}