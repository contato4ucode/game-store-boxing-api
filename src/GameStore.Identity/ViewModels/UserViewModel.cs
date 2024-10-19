using System.ComponentModel.DataAnnotations;

namespace GameStore.Identity.ViewModels;

public class RegisterUserViewModel
{
    [Required(ErrorMessage = "The field {0} is required")]
    [EmailAddress(ErrorMessage = "The field {0} is in an invalid format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The field {0} is required")]
    [StringLength(100, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginUserViewModel
{
    [Required(ErrorMessage = "The field {0} is required")]
    [EmailAddress(ErrorMessage = "The field {0} is in an invalid format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The field {0} is required")]
    [StringLength(100, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class UserTokenViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IEnumerable<ClaimViewModel> Claims { get; set; } = Enumerable.Empty<ClaimViewModel>();
}

public class LoginResponseViewModel
{
    public string AccessToken { get; set; } = string.Empty;
    public double ExpiresIn { get; set; }
    public UserTokenViewModel UserToken { get; set; } = new();
}

public class ClaimViewModel
{
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
