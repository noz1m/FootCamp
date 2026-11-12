using Domain.Pagination;

namespace Domain.Filter;

public class StadiumPhotoFilter : ValidFilter
{
    public Guid? StadiumId { get; set; }
    public bool? IsPrimary { get; set; }
}