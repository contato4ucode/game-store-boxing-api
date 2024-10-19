namespace GameStore.Identity.ViewModels;

public class AssignRolesAndClaimsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public IEnumerable<ClaimViewModel> Claims { get; set; } = Enumerable.Empty<ClaimViewModel>();
}
