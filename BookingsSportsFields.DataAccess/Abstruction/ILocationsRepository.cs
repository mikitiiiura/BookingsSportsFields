using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.DataAccess.Repositories
{
    public interface ILocationsRepository
    {
        Task<List<LocationsEntity>> GetAll();
        Task<List<LocationsEntity>> GetAllByID(Guid sportFildId);
    }
}