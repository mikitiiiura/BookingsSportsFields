using BookingsSportsFields.Core.Model;

namespace BookingsSportsFields.Application.Contracts.Request;

public class UpdateSportsFieldDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    
    // ★★★ НОВЕ — тепер підтримує інстанси ★★★
    public List<UpdateSportTypeDetailDto>? Types { get; set; }
}



public class UpdateSportTypeDetailDto
{
    public Guid? Id { get; set; }           // ← Додай
    public SportFieldsType Type { get; set; }
    public double PricePerHour { get; set; }
    public string? WarningInformation { get; set; }
    public List<UpdateWeeklyScheduleDto> WeeklySchedules { get; set; } = new();

    // ★★★ НОВЕ ★★★
    public int? Quantity { get; set; }
    public List<UpdateInstanceDto> Instances { get; set; } = new();
}

public class UpdateInstanceDto
{
    public Guid? Id { get; set; }           // null = новий
    public string DisplayName { get; set; } = string.Empty;
}
public class UpdateWeeklyScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan AvailableFrom { get; set; }
    public TimeSpan AvailableTo { get; set; }
    // НЕМАЄ Id — бекенд сам генерує
}