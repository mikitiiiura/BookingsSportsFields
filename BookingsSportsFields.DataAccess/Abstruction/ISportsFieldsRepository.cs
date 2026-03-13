using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.DataAccess.Abstruction
{
    public interface ISportsFieldsRepository
    {
        // Task Add(SportsFieldsEntity sportsFields);
        Task<List<SportsFieldsEntity>> GetAll();
        Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId);
        //Task<List<SportsFieldsEntity>> GetFilteredFild(string? search, string? type);
        

        Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city);
        Task<SportsFieldsEntity> CreateSportsField(SportsFieldsEntity sportsFields);
        
        Task<SportsFieldsEntity?> GetByIdAsync(Guid id);
        Task DeleteTypesForFieldAsync(Guid sportsFieldId);
        Task UpdateAsync(SportsFieldsEntity entity);
        Task<SportsFieldsEntity?> GetByIdWithDetailsAsync(Guid id);

        Task ReplaceTypesAndSchedulesAsync(Guid sportsFieldId, List<SportsFieldSportTypeEntity> newTypes);

        Task SaveChangesAsync();

        Task UpdateImageUrlAsync(Guid id, string newImageUrl);
    }
}