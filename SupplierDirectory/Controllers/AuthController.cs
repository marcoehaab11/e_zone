using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SupplierDirectory.Application;
using SupplierDirectory.Domain;
namespace SupplierDirectory.Controllers;
[ApiController,Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users,IConfiguration config,ILogger<AuthController> log) : ControllerBase {
 [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest request){var user=await users.FindByEmailAsync(request.Email);if(user is null||!await users.CheckPasswordAsync(user,request.Password)){log.LogWarning("Failed login for {Email}",request.Email);return Unauthorized(new ApiResponse<object>(false,"بيانات الدخول غير صحيحة",null));}var roles=await users.GetRolesAsync(user);var claims=new List<Claim>{new(JwtRegisteredClaimNames.Sub,user.Id),new(JwtRegisteredClaimNames.Email,user.Email??"")};claims.AddRange(roles.Select(r=>new Claim(ClaimTypes.Role,r)));var jwt=config.GetSection("Jwt");var token=new JwtSecurityToken(jwt["Issuer"],jwt["Audience"],claims,expires:DateTime.UtcNow.AddHours(8),signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),SecurityAlgorithms.HmacSha256));return Ok(new ApiResponse<object>(true,"تم تسجيل الدخول",new{token=new JwtSecurityTokenHandler().WriteToken(token),expiresAt=token.ValidTo}));}
}
