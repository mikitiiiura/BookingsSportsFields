using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.Application.InterfaceServices
{
    public interface ISportFildService
    {
        Task<List<SportsFieldsEntity>> GetAll();

        Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city);

    }
}