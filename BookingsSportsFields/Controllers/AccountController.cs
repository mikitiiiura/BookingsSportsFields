using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookingsSportsFields.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AccountController(
        UserManager<UserEntity> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Модель для реєстрації
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;

        //public UserRole Role { get; set; } = UserRole.User;

        //public UserRole Role { get; set; } = UserRole.User; // Значення за замовчуванням
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized(new { Message = "Not logged in" });

        return Ok(new
        {
            Email = User.Identity.Name,
            Message = "User is authenticated"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { Errors = errors });
        }

        // Знайти користувача по email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized(new { Message = "Invalid email or password" });

        // Перевірка пароля
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
            return Unauthorized(new { Message = "Invalid email or password" });

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName
        });
    }



    // POST: api/Account/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

            return BadRequest(new { Errors = errors });
        }

        // Перевірка, чи існує користувач з таким email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest(new { Message = "User with this email already exists" });

        // Створення нового користувача
        var user = new UserEntity
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        };

        // Створення користувача
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new
        {
            Message = "User registered successfully",
            UserId = user.Id,
            UserCode = user.UserCode
        });
    }

    //// Перевірка та створення ролі, якщо її немає
    //var roleName = model.Role.ToString();
    //if (!await _roleManager.RoleExistsAsync(roleName))
    //{
    //    await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
    //}

    //// Додавання ролі користувачу
    //await _userManager.AddToRoleAsync(user, roleName);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
            return NotFound(new { Message = "User not found" });

        return Ok(new
        {
            user.Id,
            user.UserCode,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.CreatedAt
        });
    }


}
