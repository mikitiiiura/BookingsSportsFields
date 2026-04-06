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

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme);

var connectionString = builder.Configuration.GetConnectionString("WebAppDbContext")
    ?? throw new InvalidOperationException(
        "Set ConnectionStrings:WebAppDbContext (e.g. user secrets or appsettings.Development.json) — point it at your Render Postgres.");

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
