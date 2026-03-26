namespace BookingsSportsFields.DataAccess.ModelEntity;

public class SportsFieldInstanceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SportsFieldId { get; set; }     // ← було Guid, стало Guid?
    public Guid SportTypeId { get; set; }
    public string DisplayName { get; set; } = string.Empty;     // "Стіл 5", "Корт A", "Поле №2"
    public bool IsActive { get; set; } = true;

    // Навігація
    public SportsFieldsEntity? SportsField { get; set; }
    public SportsFieldSportTypeEntity? SportType { get; set; }
    public ICollection<BookingsEntity> Bookings { get; set; } = new List<BookingsEntity>();
}