using System.Security.Claims;
using AuthApi.Models;

namespace AuthApi;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser applicationUser,IEnumerable<string> roles);
}