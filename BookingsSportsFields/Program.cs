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

static string ResolvePostgresConnectionString(IConfiguration config)
{
    var databaseUrl = config["DATABASE_URL"];

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        try
        {
            var uri = new Uri(databaseUrl);

            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo[0];
            var password = userInfo[1];

            var host = uri.Host;
            var port = uri.Port;
            var database = uri.AbsolutePath.Trim('/');

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to parse DATABASE_URL from Render.", ex);
        }
    }

    return config.GetConnectionString("WebAppDbContext")
        ?? throw new InvalidOperationException("No database connection configured.");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

// Cookie auth
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

// Cookie config (ВАЖЛИВО для браузера)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var connectionString = ResolvePostgresConnectionString(builder.Configuration);

builder.Services.AddDbContext<BookingsSportsFieldsDBContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public"));

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging().EnableDetailedErrors();
});

builder.Services.AddIdentityCore<UserEntity>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<BookingsSportsFieldsDBContext>()
    .AddApiEndpoints();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<ISportsFieldsRepository, SportsFieldsRepository>();
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();
builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>();

builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<ISportFildService, SportFildService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddHostedService<BookingStatusUpdater>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "http://localhost:5000",
            "http://172.25.160.1:5173",
            "http://172.20.10.3:5173",
            "http://192.168.0.51:5173",
            "http://192.168.0.103:5000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// 🔥 Автоматичні міграції
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingsSportsFieldsDBContext>();
    db.Database.Migrate();
}

// Static files
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images")),
    RequestPath = "/images"
});

// 🔥 Swagger ВКЛЮЧЕНИЙ ЗАВЖДИ
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

// ❗ ВИМКНУТО (Render сам дає HTTPS)
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<UserEntity>();

app.MapGet("user/me", async (ClaimsPrincipal claims, BookingsSportsFieldsDBContext context) =>
{
    Guid userId = Guid.Parse(claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    return await context.Users.FindAsync(userId);
})
.RequireAuthorization();

app.MapControllers();

app.Run();



/* using BookingsSportsFields;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Application.Services;
using BookingsSportsFields.Application.ServicesForEmail;
using BookingsSportsFields.DataAccess;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;
using BookingsSportsFields.Application.Services.Hosted_Service;
using Microsoft.Extensions.FileProviders;

static string ResolvePostgresConnectionString(IConfiguration config)
{
    var url = config["DATABASE_URL"];
    if (!string.IsNullOrWhiteSpace(url))
    {
        try
        {
            var csb = new NpgsqlConnectionStringBuilder(url)
            {
                SslMode = SslMode.Require
            };
            return csb.ConnectionString;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to parse DATABASE_URL. Check the value in Render Environment.", ex);
        }
    }

    return config.GetConnectionString("WebAppDbContext")
        ?? throw new InvalidOperationException(
            "No database connection: set DATABASE_URL or ConnectionStrings__WebAppDbContext.");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

// === ТІЛЬКИ COOKIE AUTHENTICATION (без JWT) ===
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

var connectionString = ResolvePostgresConnectionString(builder.Configuration);

builder.Services.AddDbContext<BookingsSportsFieldsDBContext>(options =>
{
    // Історія міграцій у public: схема "identity" з’являється лише після Up(), інакше GetAppliedMigrations() падає на першому запуску.
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging().EnableDetailedErrors();
});

builder.Services.AddIdentityCore<UserEntity>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<BookingsSportsFieldsDBContext>()
    .AddApiEndpoints();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<ISportsFieldsRepository, SportsFieldsRepository>();
builder.Services.AddScoped<IBookingsRepository, BookingsRepository>();
builder.Services.AddScoped<IReviewsRepository, ReviewsRepository>(); // ← додай, якщо ще немає

builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<ISportFildService, SportFildService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReviewService, ReviewService>();   // ← додай

builder.Services.AddHostedService<BookingStatusUpdater>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "http://localhost:5000",
            "http://172.25.160.1:5173",
            "http://172.20.10.3:5173",
            "http://192.168.0.51:5173",
            "http://192.168.0.103:5000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingsSportsFieldsDBContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "images")),
    RequestPath = "/images"
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.MapIdentityApi<UserEntity>();

app.MapGet("user/me", async (ClaimsPrincipal claims, BookingsSportsFieldsDBContext context) =>
{
    Guid userId = Guid.Parse(claims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    return await context.Users.FindAsync(userId);
})
.RequireAuthorization();

app.UseAuthorization();
app.MapControllers();

app.Run(); */