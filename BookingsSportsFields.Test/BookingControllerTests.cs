using System.Text.Json;
using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Controllers;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingsSportsFields.Test;

public class BookingControllerTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly Mock<ILogger<BookingController>> _mockLogger;
    private readonly BookingController _controller;

    public BookingControllerTests()
    {
        _mockBookingService = new Mock<IBookingService>();
        _mockLogger = new Mock<ILogger<BookingController>>();
        _controller = new BookingController(_mockBookingService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByUserId_ReturnsOkResult_WithBookingList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fakeBookings = new List<BookingsEntity>
        {
            new BookingsEntity { Id = Guid.NewGuid(), Comment = "Test 1" },
            new BookingsEntity { Id = Guid.NewGuid(), Comment = "Test 2" }
        };

        _mockBookingService
            .Setup(s => s.GetBookingByUser(userId))
            .ReturnsAsync(fakeBookings);//Прописуємо: коли сервіс отримає userId — поверни наш список.

        // Act
        var result = await _controller.GetByUserId(userId);//Викликаємо метод контролера.

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var bookings = Assert.IsType<List<BookingResponse>>(okResult.Value);

        Assert.Equal(2, bookings.Count);
        Assert.Equal("Test 1", bookings[0].Comment);
    }

    [Fact]
    public async Task CancellationBooking_ReturnsNoContent()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        
        _mockBookingService
            .Setup(s => s.CancellationBooking(bookingId)).ReturnsAsync(Guid.NewGuid());

        // Act
        var result = await _controller.CancellationBooking(bookingId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CancellationBooking_ReturnsNotFound_WhenBookingNotFound()
    {
        //Arrange
        var bookingId = Guid.NewGuid();

        _mockBookingService.Setup(s => s.CancellationBooking(bookingId)).ThrowsAsync(new KeyNotFoundException());
        //Act
        var result = await _controller.CancellationBooking(bookingId);
        
        //Assert
        Assert.IsType<NotFoundResult>(result);
    }
 
    [Fact]
    public async Task CancellationBooking_ReturnsInternalServerError_WhenExceptionThrown()
    {
        //Arrange
        var bookingId = Guid.NewGuid();

        _mockBookingService.Setup(s => s.CancellationBooking(bookingId)).ThrowsAsync(new Exception("Something went wrong"));
        //Act
        var result = await _controller.CancellationBooking(bookingId);
        
        //Assert
        
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}