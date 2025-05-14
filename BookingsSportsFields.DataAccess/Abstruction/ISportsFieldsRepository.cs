using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.DataAccess.Abstruction
{
    public interface ISportsFieldsRepository
    {
        Task Add(SportsFieldsEntity sportsFields);
        Task<List<SportsFieldsEntity>> GetAll();
        Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId);
        //Task<List<SportsFieldsEntity>> GetFilteredFild(string? search, string? type);
        Task Update(SportsFieldsEntity sportsFilds);

        Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city);
    }
}