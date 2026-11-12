namespace Infrastructure.RedisCache.RedisCacheService;

public class CacheKeys
{
    // Stadiums
    public static string AllStadiums => "stadiums:all";
    public static string StadiumById(Guid id) => $"stadiums:{id}";
    public static string OwnerStadiums(string ownerId) => $"stadiums:owner:{ownerId}";
    public static string StadiumReviews(Guid stadiumId) => $"stadiums:{stadiumId}:reviews";
    public static string StadiumRating(Guid stadiumId) => $"stadiums:{stadiumId}:rating";

    // Users
    public static string UserProfile(string userId) => $"users:{userId}:profile";
}
