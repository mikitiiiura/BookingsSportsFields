using System;

namespace BookingsSportsFields.Core.Model
{
    public class Discounts
    {
        public Discounts(Guid id, string code, byte discountPercentage, DateTime expireDate, bool isActive, DateTime createdAT)
        {
            Id = id;
            Code = code;
            DiscountPercentage = discountPercentage;
            ExpiryDate = expireDate;
            IsActive = isActive;
            CreatedAt = createdAT;
        }
        public Guid Id { get; } //Унікальний ідентифікатор знижки
        public string Code { get; } = string.Empty; //Промокод
        public byte DiscountPercentage { get; } //Відсоток знижки
        public DateTime ExpiryDate { get; } //Дата закінчення дії знижки
        public bool IsActive { get; }  //Чи активна знижка
        public DateTime CreatedAt { get; } //Дата створення знижки
    }
}
