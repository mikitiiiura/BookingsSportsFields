using BookingsSportsFields;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Application.Services;
using BookingsSportsFields.Application.ServicesForEmail;
using BookingsSportsFields.DataAccess;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookingsSportsFields.Application.Services.Hosted_Service;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);


var connectionString = builder.Configuration.GetConnectionString("WebAppDbContext");
// builder.Services.AddDbContext<BookingsSportsFieldsDBContext>(options =>
//     options.UseSqlServer(connectionString));

builder.Services.AddDbContext<BookingsSportsFieldsDBContext>(options =>
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());

builder.Services.AddIdentityCore<UserEntity>()
    .AddRoles<IdentityRole<Guid>>() // Додаємо ролі з Guid
    .AddEntityFrameworkStores<BookingsSportsFieldsDBContext>()
    .AddApiEndpoints();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));



builder.Services.AddScoped<ISportsFieldsRepository, SportsFieldsRepository>();
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();

builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<ISportFildService, SportFildService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddHostedService<BookingStatusUpdater>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173", "http://localhost:5000", "http://192.168.0.103:5000") // Дозволяє твоєму React-додатку робити запити Важливо без /
                        .AllowAnyHeader()
                        .AllowAnyMethod()//);
                        .AllowCredentials()); // Якщо використовуєш аутентифікацію через cookies
}); 

var app = builder.Build();
app.UseStaticFiles(); // save image from backend

// Якщо хочеш, щоб папка images була доступна з /images (а не з /wwwroot/images)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images")),
    RequestPath = "/images"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.MapIdentityApi<UserEntity>();

app.MapGet("user/me", async(ClaimsPrincipal claims, BookingsSportsFieldsDBContext context) =>
{
    Guid userId = Guid.Parse(claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

    return await context.Users.FindAsync(userId);
})
    .RequireAuthorization();

app.UseAuthorization();

app.MapControllers();

app.Run();
