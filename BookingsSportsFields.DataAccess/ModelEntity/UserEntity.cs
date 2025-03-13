using BookingsSportsFields.Core.Model;
using Microsoft.AspNetCore.Identity;
using System;

namespace BookingsSportsFields.DataAccess.ModelEntity
{
    public class UserEntity : IdentityUser<Guid>
    {
        ///// <summary>
        ///// Унікальний ідентифікатор користувача
        ///// </summary>
        //public Guid Id { get; set; }
        /// <summary>
        /// Повне ім'я користувача
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        ///// <summary>
        ///// Email(логін)
        ///// </summary>
        //public string Email { get; set; } = string.Empty;
        ///// <summary>
        ///// Хеш пароля
        ///// </summary>
        //public string Passwordhash { get; set; } = string.Empty;
        /// <summary>
        /// Промокод відповідного користувача для запрошення друзів
        /// </summary>
        public string UserCode { get; set; } = string.Empty;
        /// <summary>
        /// Роль користувача
        /// </summary>
        public UserRole Role { get; set; }
        /// <summary>
        /// Дата створення акаунта
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
