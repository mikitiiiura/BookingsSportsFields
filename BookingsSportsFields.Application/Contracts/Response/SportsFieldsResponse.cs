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
                sportsFields.Location != null ? new LocationDto(sportsFields.Location.Id, sportsFields.Location.Latitude, sportsFields.Location.Longitude, sportsFields.Location.Address) : null!
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
        string Address
    );

    public record OwnerDto
    (
        Guid Id,
        string Name
        // Додайте інші необхідні поля
    );

}
