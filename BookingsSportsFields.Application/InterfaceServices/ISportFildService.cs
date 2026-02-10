using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.Application.InterfaceServices
{
    public interface ISportFildService
    {
        Task<List<SportsFieldsEntity>> GetAll();

        Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId);

        Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city);
        Task<SportsFieldsEntity> AddSportsFields(SportsFieldsEntity sportsFields);
        
       // Task<SportsFieldsEntity?> GetById(Guid id);
        Task UpdateAsync(UpdateSportsFieldDto dto);

    }
}