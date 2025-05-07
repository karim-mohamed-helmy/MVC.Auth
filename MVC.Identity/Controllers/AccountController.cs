using System.Security.Claims;
using MCV.Identity.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Identity.Data.Models;
using MVC.Identity.Dtos;

namespace MVC.Identity.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;

        public AccountController(UserManager<CustomUser> userManager , SignInManager<CustomUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        /**********************************************************************************/
        //register
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Register(RegisterDto registerDto)
        {
            if(!ModelState.IsValid)
            {
                return View(registerDto);
            }
            var user = new CustomUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                CustomProperty = "static data"
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return View(registerDto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var claimResult = await _userManager.AddClaimsAsync(user, claims);

            return RedirectToAction("Login");
        }
        /**********************************************************************************/
        //login 
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginCredentialsDto credentials)
        {
            if (!ModelState.IsValid)
            {
                return View(credentials);
            }

            var user = await _userManager.FindByNameAsync(credentials.UserName);
            if (user is null)
            {
                return View(credentials);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, credentials.Password);
            if (!passwordValid)
            {
                return View(credentials);
            }

            var claims = await _userManager.GetClaimsAsync(user);

            await _signInManager.SignInWithClaimsAsync(user, false, claims);

            return RedirectToAction("Index", "Home");
        }
        /**********************************************************************************/
        //logout
        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
