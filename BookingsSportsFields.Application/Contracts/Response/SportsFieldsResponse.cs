using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.Contracts.Response
{
    public record SportTypeDetailDto
    (
        SportFieldsType Type,
        double PricePerHour,
        string? WarningInformation,
        List<WeeklyScheduleDto> WeeklySchedules,
    
        // ★★★ НОВЕ — обов’язково додати ★★★
        List<InstanceDto> Instances
    );
    public record InstanceDto
    (
        Guid Id,
        string DisplayName,
        bool IsActive = true   // опціонально, якщо хочеш передавати статус
    );
    // public record InstanceDto(Guid Id, string DisplayName);
    public record WeeklyScheduleDto
    (
        DayOfWeek DayOfWeek,       // 0 = неділя, 1 = понеділок, і т.д.
        TimeSpan AvailableFrom, // формат "HH:mm", наприклад "08:00"
        TimeSpan AvailableTo
    );


    
    public record SportsFieldResponce
    (
        Guid Id,
        string Title,
        List<SportTypeDetailDto> Types,
        string Description,
        //DateTime CreatedAt,
        string ImageUrl,
        LocationDto Location,
        OwnerDto? Owner
    )
    {
        public SportsFieldResponce(SportsFieldsEntity sportsFields) : this
            (
                sportsFields.Id,
                sportsFields.Name,
                
                sportsFields.TypesWithDetails.Select(t => new SportTypeDetailDto(
                    t.Type,
                    t.PricePerHour,
                    t.WarningInformation,
                    t.WeeklySchedules.Select(ws => new WeeklyScheduleDto(
                        ws.DayOfWeek,
                        ws.AvailableFrom,
                        ws.AvailableTo
                    )).ToList(),
            
                    // ★★★ Це саме те, що потрібно додати ★★★
                    t.Instances.Select(i => new InstanceDto(
                        i.Id,
                        i.DisplayName,
                        i.IsActive
                    )).ToList()
                )).ToList(),
                sportsFields.Description,
                //sportsFields.CreatedAt,
                sportsFields.ImageUrl,
                sportsFields.Location != null ? new LocationDto(sportsFields.Location.Id, sportsFields.Location.Latitude, sportsFields.Location.Longitude, sportsFields.Location.Address, sportsFields.Location.City) : null!,
                sportsFields.Owner != null ? new OwnerDto(sportsFields.Owner.Id, sportsFields.Owner.FullName) : null!
            )
        {
        }
    }
    
    
    public record SportsFieldByUser
    (
        Guid Id,
        string Title,
        List<SportTypeDetailDto> Types,
        string ImageUrl,
        OwnerDto? Owner
    )
    {
        public SportsFieldByUser(SportsFieldsEntity sportsFields) : this
        (
            sportsFields.Id,
            sportsFields.Name,
            sportsFields.TypesWithDetails.Select(t => new SportTypeDetailDto(
                t.Type,
                t.PricePerHour,
                t.WarningInformation,
                t.WeeklySchedules.Select(ws => new WeeklyScheduleDto(
                    ws.DayOfWeek,
                    ws.AvailableFrom,
                    ws.AvailableTo
                )).ToList(),
            
                // ★★★ Додаємо Instances ★★★
                t.Instances.Select(i => new InstanceDto(
                    i.Id,
                    i.DisplayName,
                    i.IsActive
                )).ToList()
            )).ToList(),
            sportsFields.ImageUrl,
            sportsFields.Owner != null ? new OwnerDto(sportsFields.Owner.Id, sportsFields.Owner.FullName) : null!
        )
        {
        }
    }

    public record LocationDto
    (
        Guid Id,
        decimal Latitude,
        decimal Longitude,
        string Address,
        string City
    );

    public record OwnerDto
    (
        Guid Id,
        string Name
    // Додайте інші необхідні поля
    );

    public record UserDto
        (
        Guid Id,
        string Name,
        string Email,
        string PhoneNumber
        );

    public record SportsFieldDto
    (
        Guid Id,
        string Name
    );

    public record BookingResponse
(
    Guid Id,
    string? Comment,
    SportFieldsType SportType,
    DateTime StartTime,
    DateTime EndTime,
    BookingStatus Status,
    decimal TotalPrice,
    DateTime CreatedAt,
    UserDto User,
    SportsFieldResponce SportsField,
    InstanceDto? SportsFieldInstance   // ← ДОДАЙ ЦЕ ПОЛЕ!
)
{
    public BookingResponse(BookingsEntity bookings) : this
    (
        bookings.Id,
        bookings.Comment,
        bookings.SportType,
        bookings.StartTime,
        bookings.EndTime,
        bookings.Status,
        bookings.TotalPrice,
        bookings.CreatedAt,
        bookings.User != null ? new UserDto(bookings.User.Id, bookings.User.FullName, bookings.User.Email, bookings.User.PhoneNumber) : null!,
        bookings.SportsField != null ? new SportsFieldResponce
        (
            bookings.SportsField.Id,
            bookings.SportsField.Name,
            bookings.SportsField.TypesWithDetails.Select(t => new SportTypeDetailDto(
                t.Type,
                t.PricePerHour,
                t.WarningInformation,
                t.WeeklySchedules.Select(ws => new WeeklyScheduleDto(
                    ws.DayOfWeek,
                    ws.AvailableFrom,
                    ws.AvailableTo
                )).ToList(),
                t.Instances.Select(i => new InstanceDto(
                    i.Id,
                    i.DisplayName,
                    i.IsActive
                )).ToList()
            )).ToList(),
            bookings.SportsField.Description,
            bookings.SportsField.ImageUrl,
            bookings.SportsField.Location != null ? new LocationDto(
                bookings.SportsField.Location.Id,
                bookings.SportsField.Location.Latitude,
                bookings.SportsField.Location.Longitude,
                bookings.SportsField.Location.Address,
                bookings.SportsField.Location.City
            ) : null!,
            bookings.SportsField.Owner != null ? new OwnerDto(bookings.SportsField.Owner.Id, bookings.SportsField.Owner.FullName) : null!
        ) : null!,
        bookings.SportsFieldInstance != null 
            ? new InstanceDto(bookings.SportsFieldInstance.Id, bookings.SportsFieldInstance.DisplayName, bookings.SportsFieldInstance.IsActive) 
            : null
    )
    {
    }
}


}
