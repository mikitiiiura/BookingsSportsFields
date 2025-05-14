using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.Contracts.Request
{
    //public record CreateBookingWithOutIdentityUser
    //(
    //    [Required(ErrorMessage = "SportFild id is required.")]
    //    Guid SportFildId,

    //    [StringLength(255, ErrorMessage = "Comment cannot be longer than 255 characters.")]
    //    string? Comment,

    //    [Required(ErrorMessage = "StartTime is required.")]
    //    DateTime StartTime,

    //    //[Required(ErrorMessage = "EndTime is required.")]
    //    //DateTime EndTime,

    //    [Required(ErrorMessage = "EndTime is required.")]
    //    DateTime EndTime,

    //    [Range(0, 3, ErrorMessage = "Priority must be between 0 and 3.")]
    //    int Status,

    //    [Required(ErrorMessage = "TotalPrice is required.")]
    //    decimal TotalPrice,

    //    Guid UserId


    //);
    public record CreateBookingRequest
    (
    [Required] Guid SportFieldId,
    [StringLength(255)] string? Comment,
    [Required] DateTime StartTime,
    [Required] int DurationMinutes, // Тривалість у хвилинах
    [Required] decimal TotalPrice,
    [Required] Guid UserId // Вже відомий, оскільки користувач зареєстрований
    );
    public record CreateGuestBookingRequest
    (
    [Required] Guid SportFieldId,
    [StringLength(255)] string? Comment,
    [Required] DateTime StartTime,
    [Required] int DurationMinutes,
    [Required] decimal TotalPrice,
    [Required] string FullName, // Контактна інформація
    [Required] string PhoneNumber
    );
}

