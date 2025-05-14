using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.Contracts.Response
{
    public record SportsFieldResponce
    (
        Guid Id,
        string Title,
        string? WarningInformation,
        SportFieldsType Type,
        double PricePerHour,
        string Description,
        //DateTime CreatedAt,
        string ImageUrl,
        LocationDto Location
    //OwnerDto? Owner
    )
    {
        public SportsFieldResponce(SportsFieldsEntity sportsFields) : this
            (
                sportsFields.Id,
                sportsFields.Name,
                sportsFields.WarningInformation,
                sportsFields.Type,
                sportsFields.PricePerHour,
                sportsFields.Description,
                //sportsFields.CreatedAt,
                sportsFields.ImageUrl,
                sportsFields.Location != null ? new LocationDto(sportsFields.Location.Id, sportsFields.Location.Latitude, sportsFields.Location.Longitude, sportsFields.Location.Address, sportsFields.Location.City) : null!
            //sportsFields.Owner != null ? new OwnerDto(sportsFields.Owner.Id, sportsFields.Owner.FullName) : null!
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
        string Name
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
        DateTime StartTime,
        DateTime EndTime,
        BookingStatus Status,
        decimal TotalPrice,
        DateTime CreatedAt,
        UserDto User,
        SportsFieldResponce SportsField
    )
    {
        public BookingResponse(BookingsEntity bookings) : this
            (
                bookings.Id,
                bookings.Comment,
                bookings.StartTime,
                bookings.EndTime,
                bookings.Status,
                bookings.TotalPrice,
                bookings.CreatedAt,
                bookings.User != null ? new UserDto(bookings.User.Id, bookings.User.FullName) : null!,
                bookings.SportsField != null ? new SportsFieldResponce
            (
                    bookings.SportsField.Id,
                    bookings.SportsField.Name,
                    bookings.SportsField.WarningInformation,
                    bookings.SportsField.Type,
                    bookings.SportsField.PricePerHour,
                    bookings.SportsField.Description,
                    bookings.SportsField.ImageUrl,
                    bookings.SportsField.Location != null ? new LocationDto(
                        bookings.SportsField.Location.Id,
                        bookings.SportsField.Location.Latitude,
                        bookings.SportsField.Location.Longitude,
                        bookings.SportsField.Location.Address,
                        bookings.SportsField.Location.City
                    )
                    : null!

                    ) : null!
            )
        {
        }
    }


}
