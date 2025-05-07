using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Data.Models;
using Identity.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<CustomUser> _userManager;

        public AccountController(IConfiguration configuration , UserManager<CustomUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("dummy-login")]
        public async Task<Results<Ok<TokenDto>, UnauthorizedHttpResult>> DummyLogin(LoginCredentialsDto loginCredentialsDto)
        {
            if (loginCredentialsDto.UserName != "test" || loginCredentialsDto.Password != "1234")
            {
                return TypedResults.Unauthorized();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim (ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, $"{Guid.NewGuid()}")
            };

            var tokenDto = GenerateToken(claims);
            return TypedResults.Ok(tokenDto);
        }

        [HttpPost]
        [Route("login")]
        public async Task<Results<Ok<TokenDto>, UnauthorizedHttpResult>> Login(LoginCredentialsDto loginCredentialsDto)
        {
            var user = await _userManager.FindByNameAsync(loginCredentialsDto.UserName);
            if (user is null)
            {
                return TypedResults.Unauthorized();
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginCredentialsDto.Password);
            if (!isPasswordValid)
            {
                return TypedResults.Unauthorized();
            }

            var claims = await _userManager.GetClaimsAsync(user);

            var tokenDto = GenerateToken(claims.ToList());

            return TypedResults.Ok(tokenDto);
        }

        [HttpPost]
        [Route("register")]
        public async Task<Results<NoContent , BadRequest<List<string>>>> Register(RegisterDto registerDto)
        {
            var user = new CustomUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                CustomProperty = registerDto.CustomProperty
            };

            //hash password and give it back to user
            var creationResult = await _userManager.CreateAsync(user, registerDto.Password);
            if (!creationResult.Succeeded)
            {
                var errors = creationResult.Errors.Select(e => e.Description).ToList();
                return TypedResults.BadRequest(errors);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.CustomProperty)
            };

            await _userManager.AddClaimsAsync(user, claims);
            return TypedResults.NoContent();
        }

        private TokenDto GenerateToken(List<Claim> claims)
        {
            var secretKey = _configuration.GetValue<string>("SecretKey")!;
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(secretKeyBytes);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenDto(tokenString, token.ValidTo);

        }
    }
}
