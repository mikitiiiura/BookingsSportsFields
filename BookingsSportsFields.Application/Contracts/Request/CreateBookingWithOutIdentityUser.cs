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
    public record CreateBookingRequest
    (
    [Required] Guid SportFieldId,
    Guid? SportsFieldInstanceId,     // ← НОВЕ
    [StringLength(255)] string? Comment,
    [Required] SportFieldsType SportType,
    [Required] DateTime StartTime,
    [Required] int DurationMinutes, // Тривалість у хвилинах
    [Required] decimal TotalPrice,
    [Required] Guid UserId // Вже відомий, оскільки користувач зареєстрований
    );
    public record CreateGuestBookingRequest
    (
    [Required] Guid SportFieldId,
    [StringLength(255)] string? Comment,
    [Required] SportFieldsType SportType,
    [Required] DateTime StartTime,
    [Required] int DurationMinutes,
    [Required] decimal TotalPrice,
    [Required] string FullName, // Контактна інформація
    [Required] string PhoneNumber
    );
}

