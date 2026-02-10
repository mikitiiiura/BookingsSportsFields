using BookingsSportsFields.Core.Model;

namespace BookingsSportsFields.Application.Contracts.Request;

public class UpdateSportsFieldDto
{
    public Guid Id { get; set; }                    // Обов'язково — щоб знати який майданчик оновлювати
    public string? Name { get; set; }               // можна не передавати — поле не зміниться
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }           // просто новий URL (наприклад з Cloudinary / S3)
    
    // Якщо хочеш змінити типи майданчика — передай новий список (старий буде повністю замінено)
    public List<UpdateSportTypeDetailDto>? Types { get; set; }
}

public class UpdateSportTypeDetailDto
{
    public SportFieldsType Type { get; set; }
    public double PricePerHour { get; set; }
    public string? WarningInformation { get; set; }
    public List<UpdateWeeklyScheduleDto> WeeklySchedules { get; set; } = new();
}

public class UpdateWeeklyScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan AvailableFrom { get; set; }
    public TimeSpan AvailableTo { get; set; }
    // НЕМАЄ Id — бекенд сам генерує
}