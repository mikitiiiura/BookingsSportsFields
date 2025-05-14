using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Core.Model
{
    public record CheckAvailabilityRequest
    (
        Guid SportFieldId,
        DateTime Date
    );

    public record AvailabilityResponse
    (
        bool IsAvailable,
        List<TimeSlotAA> AvailableSlots
    );

    public record TimeSlotAA
    (
        DateTime StartTime,
        DateTime EndTime,
        bool IsAvailable
    );
}
