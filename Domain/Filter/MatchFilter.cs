using Domain.Pagination;

namespace Domain.Filter;

public class MatchFilter : ValidFilter
{
    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }
    public string? Status { get; set; }    // open, confirmed, cancelled
    public Guid? StadiumId { get; set; }
    public string? CreatorId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
