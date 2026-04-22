using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    
    public class UpdateUserProfileModel
    {
        public Guid IdUser { get; set; }
        public string? NewEmail { get; set; }
        public string? NewPhoneNumber { get; set; }
        public string? NewFullName { get; set; }
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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName ?? user.Email ?? user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName
        });
    }
    
    [HttpPost("login-crm")]
    public async Task<IActionResult> LoginCRM([FromBody] LoginModel model)
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
        
        // Перевірка ролі адміністратора спортивних майданчиків
        if (user.Role != UserRole.AdminSportsFields)
            return Unauthorized(new { Message = "Access denied. User is not an AdminSportsFields." });
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName ?? user.Email ?? user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.Id,
            UserCode = user.UserCode,
            FullName = user.FullName
        });
    }
    
    // POST: api/Account/registerCRM
    [HttpPost("register-crm")]
    public async Task<IActionResult> RegisterCRM([FromBody] RegisterModel model)
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
            return BadRequest(new { Message = "User(AdminSportFild) with this email already exists" });

        // Створення нового користувача
        var user = new UserEntity
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Role = UserRole.AdminSportsFields,
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

    // [HttpPost("change-fullname")]
    // public async Task<IActionResult> ChangeFullName(Guid userId,  string newFullName)
    // {
    //     var user = await _userManager.FindByIdAsync(userId.ToString());
    //     if (user == null)
    //         return Unauthorized(new { Message = "User Not Found" });
    //     user.FullName = newFullName;
    //     var result = await _userManager.UpdateAsync(user);
    //     if (!result.Succeeded)
    //         return BadRequest(result.Errors);
    //     return Ok(new
    //     {
    //         user.Id,
    //         user.UserCode,
    //         user.FullName,
    //         user.Email,
    //         user.PhoneNumber,
    //         user.Role,
    //         user.CreatedAt
    //     });
    // }

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
    
    
    [HttpPost("update-profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileModel model)
    {
        var user = await _userManager.FindByIdAsync(model.IdUser.ToString());
        if (user == null)
            return NotFound(new { Message = "User not found" });

        bool isModified = false;
        var changedFields = new List<string>();
        var unchangedFields = new List<string>();

        // Email
        if (!string.IsNullOrWhiteSpace(model.NewEmail))
        {
            if (user.Email != model.NewEmail)
            {
                user.Email = model.NewEmail;
                changedFields.Add("Email");
                isModified = true;
            }
            else
            {
                unchangedFields.Add("Email");
            }
        }

        // Phone number
        if (!string.IsNullOrWhiteSpace(model.NewPhoneNumber))
        {
            if (user.PhoneNumber != model.NewPhoneNumber)
            {
                user.PhoneNumber = model.NewPhoneNumber;
                changedFields.Add("PhoneNumber");
                isModified = true;
            }
            else
            {
                unchangedFields.Add("PhoneNumber");
            }
        }

        // Full name
        if (!string.IsNullOrWhiteSpace(model.NewFullName))
        {
            if (user.FullName != model.NewFullName)
            {
                user.FullName = model.NewFullName;
                changedFields.Add("FullName");
                isModified = true;
            }
            else
            {
                unchangedFields.Add("FullName");
            }
        }

        if (!isModified)
        {
            return Ok(new
            {
                Message = "Дані залишились такими, як були — жодних змін не внесено.",
                UnchangedFields = unchangedFields
            });
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new
        {
            Message = "Дані користувача успішно оновлено.",
            ChangedFields = changedFields,
            UnchangedFields = unchangedFields
        });
    }


    // [HttpPost("change-email")]
    // public async Task<IActionResult> changreEmail(Guid idUser, string newEmail)
    // {
    //     var user = await _userManager.FindByIdAsync(idUser.ToString());
    //     
    //     if (user == null)
    //         return NotFound(new { Message = "User not found" });
    //     
    //     user.Email = newEmail;
    //     var result = await _userManager.UpdateAsync(user);
    //     if (!result.Succeeded)
    //         return BadRequest(result.Errors);
    //     return Ok(idUser);
    // }
    //
    // [HttpPost("change-phone-number")]
    // public async Task<IActionResult> changrePhoneNumber(Guid idUser, string newPhoneNumber)
    // {
    //     var user = await _userManager.FindByIdAsync(idUser.ToString());
    //     
    //     if (user == null)
    //         return NotFound(new { Message = "User not found" });
    //     
    //     user.PhoneNumber = newPhoneNumber;
    //     var result = await _userManager.UpdateAsync(user);
    //     
    //     if (!result.Succeeded)
    //         return BadRequest(result.Errors);
    //     
    //     return Ok(idUser);
    // }
    //
    // [HttpPost("change-fullname")]
    // public async Task<IActionResult> changreFullName(Guid idUser, string newFullName)
    // {
    //     var user = await _userManager.FindByIdAsync(idUser.ToString());
    //     
    //     if (user == null)
    //         return NotFound(new { Message = "User not found" });
    //     
    //     user.FullName = newFullName;
    //     var result = await _userManager.UpdateAsync(user);
    //     if (!result.Succeeded)
    //         return BadRequest(result.Errors);
    //     return Ok(idUser);
    // }


}
