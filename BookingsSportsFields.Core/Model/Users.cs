using Microsoft.VisualBasic;

namespace BookingsSportsFields.Core.Model
{
    public class Users
    {
        public Users(Guid id, string fullname, string email, string passwordhash, string usercode, UserRole role, DateTime createat)
        {
            Id = id;
            FullName = fullname;
            Email = email;
            Passwordhash = passwordhash;
            UserCode = usercode;
            Role = role;
            CreatedAt = createat;
        }
        Guid Id { get; } //Унікальний ідентифікатор користувача
        string FullName { get; } = string.Empty; // Повне ім'я користувача
        public string Email { get; } = string.Empty; // Email(логін)
        public string Passwordhash { get; } = string.Empty; // Хеш пароля
        public string UserCode { get; } = string.Empty; // Промокод відповідного користувача для запрошення друзів
        public UserRole Role { get; }// Роль користувача
        public DateTime CreatedAt { get; }// Дата створення акаунта
    }

    public enum UserRole
    {
        User = 1,
        AdminSportsFields = 2,
        Admin = 3
    }
}
