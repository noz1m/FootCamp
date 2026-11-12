using System.Reflection;
using Domain.Entities;
using Domain.Pagination;
using Infrastructure.BackgroundTasks;
using Infrastructure.Bookings.Interfaces;
using Infrastructure.Bookings.Repositories;
using Infrastructure.Bookings.Services;
using Infrastructure.Data;
using Infrastructure.MatchRequests.Interfaces;
using Infrastructure.MatchRequests.Repositories;
using Infrastructure.MatchRequests.Services;
using Infrastructure.Matches.Interfaces;
using Infrastructure.Matches.Repositories;
using Infrastructure.Matches.Services;
using Infrastructure.Payments.Interfaces;
using Infrastructure.Payments.Repositories;
using Infrastructure.Payments.Services;
using Infrastructure.Reviews.Interfaces;
using Infrastructure.Reviews.Repositories;
using Infrastructure.Reviews.Services;
using Infrastructure.Services.Auth.AuthInterface;
using Infrastructure.Services.Auth.AuthService;
using Infrastructure.Services.Email.EmailInterface;
using Infrastructure.Services.Email.EmailService;
using Infrastructure.RedisCache;
using Infrastructure.RedisCache.RedisCacheService;
using Infrastructure.StadiumPhotos.Interfaces;
using Infrastructure.StadiumPhotos.Repositories;
using Infrastructure.StadiumPhotos.Services;
using Infrastructure.Stadiums.Interfaces;
using Infrastructure.Stadiums.Repositories;
using Infrastructure.Stadiums.Services;
using Infrastructure.Users.Interfaces;
using Infrastructure.Users.Repositories;
using Infrastructure.Users.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ===== DATABASE =====
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // ===== AUTOMAPPER =====
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // ===== REDIS =====
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            var configOptions = ConfigurationOptions.Parse(redisConnection);
            configOptions.AbortOnConnectFail = false; // не падаем если Redis недоступен
            configOptions.ConnectTimeout = 3000;
            configOptions.SyncTimeout = 3000;
            return ConnectionMultiplexer.Connect(configOptions);
        });
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        // ===== PAGINATION =====
        services.AddScoped<Pagination<Stadium>>();
        services.AddScoped<Pagination<StadiumPhoto>>();
        services.AddScoped<Pagination<Booking>>();
        services.AddScoped<Pagination<Payment>>();
        services.AddScoped<Pagination<Review>>();
        services.AddScoped<Pagination<ApplicationUser>>();
        services.AddScoped<Pagination<Match>>();
        services.AddScoped<Pagination<MatchRequest>>();

        // ===== REPOSITORIES =====
        services.AddScoped<IStadiumRepository, StadiumRepository>();
        services.AddScoped<IStadiumPhotoRepository, StadiumPhotoRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IMatchRequestRepository, MatchRequestRepository>();

        // ===== SERVICES =====
        services.AddScoped<IStadiumService, StadiumService>();
        services.AddScoped<IStadiumPhotoService, StadiumPhotoService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IMatchRequestService, MatchRequestService>();

        // ===== AUTH & EMAIL =====
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();

        // ===== BACKGROUND TASKS =====
        services.AddHostedService<BookingReminderTask>();
        services.AddHostedService<AutoCancelUnpaidBookingsTask>();
        services.AddHostedService<CompletedBookingsUpdateTask>();

        return services;
    }
}