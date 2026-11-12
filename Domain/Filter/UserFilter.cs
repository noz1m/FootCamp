using Domain.Pagination;

namespace Domain.Filter;

public class UserFilter : ValidFilter
{
    public string? Role { get; set; }       // admin, manager, user
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}