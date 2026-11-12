using Domain.Pagination;

namespace Domain.Filter;

public class ReviewFilter : ValidFilter
{
    public Guid? StadiumId { get; set; }
    public string? UserId { get; set; }
    public int? MinRating { get; set; }    // 1-5
    public int? MaxRating { get; set; }    // 1-5
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}