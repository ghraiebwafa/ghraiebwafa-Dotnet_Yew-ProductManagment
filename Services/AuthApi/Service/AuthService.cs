using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AuthApi.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager; 
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

   
    public async Task<bool> AssignRole(string email, string roleName)
    {
        var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user != null)
        {
            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }
        return false;

    }

    public async Task<string> Register(RegistrationDto registrationDto)
    {
        ApplicationUser user = new()
        {
            UserName = registrationDto.Email,
            Email = registrationDto.Email,
            NormalizedEmail = registrationDto.Email.ToUpper(),
            Name = registrationDto.Name,
            PhoneNumber = registrationDto.PhoneNumber,
        };
        try
        {
            var result = await _userManager.CreateAsync(user, registrationDto.Password);
            if (result.Succeeded)
            {
                var assignRoleSuccess = await AssignRole(registrationDto.Email, registrationDto.Role?.ToUpper() ?? "Customer");
                if (assignRoleSuccess)
                {
                    return "";
                }
                else
                {
                    return "Role assignment failed";
                }
            }
            else
            {
                return result.Errors.FirstOrDefault()?.Description ?? "Error during registration";
            }
        }
        catch (Exception e)
        {
            return "Error!!";
        }
    }


public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
{
    var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.Username.ToLower());

    bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

    if (user == null || !isValid)
    {
        return new LoginResponseDto() { User = null, Token = "" };
    }

    var roles = await _userManager.GetRolesAsync(user);
    var token = _jwtTokenGenerator.GenerateToken(user, roles);

    UserDto userDto = new()
    {
        Email = user.Email,
        ID = user.Id,
        Name = user.Name,
        PhoneNumber = user.PhoneNumber
    };

    return new LoginResponseDto()
    {
        User = userDto,
        Token = token
    };
}


    }

