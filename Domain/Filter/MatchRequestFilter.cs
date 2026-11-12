using Domain.Pagination;

namespace Domain.Filter;

public class MatchRequestFilter : ValidFilter
{
    public Guid? MatchId { get; set; }
    public string? UserId { get; set; }
    public string? Status { get; set; }    // pending, approved, rejected, cancelled
}