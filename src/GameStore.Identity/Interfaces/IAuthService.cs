using GameStore.Identity.ViewModels;

namespace GameStore.Identity.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterUserViewModel registerUser);
    Task<LoginResponseViewModel> LoginAsync(LoginUserViewModel loginUser);
    Task<bool> AddRoleAsync(string roleName);
    Task<bool> AssignRolesAndClaimsAsync(string userId, IEnumerable<string> roles, IEnumerable<ClaimViewModel> claims);
    Task<LoginResponseViewModel> GenerateJwtAsync(string email);
}
