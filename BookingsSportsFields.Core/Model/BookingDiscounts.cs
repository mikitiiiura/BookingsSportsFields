using System;

namespace BookingsSportsFields.Core.Model
{
    public class BookingDiscounts
    {
        public BookingDiscounts(Guid id, decimal discountAmount)
        {
            Id = id;
            DiscountAmount = discountAmount;
        }

        public Guid Id { get;} //Унікальний ідентифікатор
        public decimal DiscountAmount { get;} //Сума знижки у валюті
    }
}
