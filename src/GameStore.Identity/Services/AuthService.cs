using GameStore.Domain.Interfaces.Notifications;
using GameStore.Identity.Extensions;
using GameStore.Identity.Interfaces;
using GameStore.Identity.ViewModels;
using GameStore.SharedServices.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GameStore.Identity.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;

    public AuthService(SignInManager<IdentityUser> signInManager,
                       UserManager<IdentityUser> userManager,
                       RoleManager<IdentityRole> roleManager,
                       IOptions<AppSettings> appSettings,
                       ILogger<AuthService> logger,
                       INotifier notifier) : base(notifier)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(RegisterUserViewModel registerUser)
    {
        try
        {
            var user = new IdentityUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return true;
            }
            foreach (var error in result.Errors)
            {
                HandleException(new Exception(error.Description));
            }
            return false;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            _logger.LogError(ex, "An error occurred while registering the user.");
            return false;
        }
    }

    public async Task<LoginResponseViewModel?> LoginAsync(LoginUserViewModel loginUser)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {loginUser.Email} logged in successfully");
                return await GenerateJwtAsync(loginUser.Email);
            }
            if (result.IsLockedOut)
            {
                HandleException(new Exception("User temporarily locked out due to multiple failed login attempts."));
            }
            else
            {
                HandleException(new Exception("Invalid user or password."));
            }
            return null;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            _logger.LogError(ex, "An error occurred while logging in the user.");
            return null;
        }
    }

    public async Task<bool> AddRoleAsync(string roleName)
    {
        try
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning($"Role {roleName} already exists.");
                return false;
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} created successfully.");
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError($"Error creating role: {error.Description}");
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while creating the role {roleName}.");
            return false;
        }
    }

    public async Task<bool> AssignRolesAndClaimsAsync(string userId, IEnumerable<string> roles, IEnumerable<ClaimViewModel> claims)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return false;
            }

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var newRole = new IdentityRole(role);
                        var roleResult = await _roleManager.CreateAsync(newRole);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                _logger.LogError($"Error creating role: {error.Description}");
                            }
                            return false;
                        }
                    }

                    if (!await _userManager.IsInRoleAsync(user, role))
                    {
                        var result = await _userManager.AddToRoleAsync(user, role);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                _logger.LogError($"Error adding role to user: {error.Description}");
                            }
                            return false;
                        }
                    }
                }
            }

            if (claims != null)
            {
                foreach (var claimViewModel in claims)
                {
                    var claim = new Claim(claimViewModel.Type, claimViewModel.Value);
                    var existingClaims = await _userManager.GetClaimsAsync(user);
                    if (!existingClaims.Any(c => c.Type == claimViewModel.Type && c.Value == claimViewModel.Value))
                    {
                        var result = await _userManager.AddClaimAsync(user, claim);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                _logger.LogError($"Error adding claim to user: {error.Description}");
                            }
                            return false;
                        }
                    }
                }
            }

            _logger.LogInformation($"Roles and/or claims assigned to user {userId} successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while assigning roles and/or claims to the user {userId}.");
            return false;
        }
    }

    public async Task<LoginResponseViewModel?> GenerateJwtAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                HandleException(new Exception("User not found."));
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Issuer,
                Audience = _appSettings.ValidAt,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encodedToken = tokenHandler.WriteToken(token);

            return new LoginResponseViewModel
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationHours).TotalSeconds,
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value }).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            HandleException(ex);
            _logger.LogError(ex, "An error occurred while generating the JWT token.");
            return null;
        }
    }

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}
