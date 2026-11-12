using System.Security.Claims;
using Infrastructure.Data;

namespace WebApi.Middleware;

public class UserActivityMiddleware(
    RequestDelegate next,
    ILogger<UserActivityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // Обновляем LastLoginAt только для авторизованных запросов
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    // DataContext — Scoped, получаем через scope
                    using var scope = context.RequestServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var user = await dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Не прерываем запрос из-за ошибки логирования активности
                    logger.LogError(ex, "Failed to update LastLoginAt for user {UserId}", userId);
                }
            }
        }
    }
}