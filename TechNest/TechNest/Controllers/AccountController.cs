using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechNest.Models;

namespace TechNest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if(!ModelState.IsValid)
            {
                return View(registerDTO);
            }

            //create a new account and authenticate the user
            var user = new ApplicationUser()
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                UserName = registerDTO.Email,       //username will be used to authenticate the user
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.PhoneNumber,
                Address = registerDTO.Address,
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                //successfull user registration
                await userManager.AddToRoleAsync(user, "client");

                //sign in the new user
                await signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // registration failed => show registration errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDTO);
        }

        public async Task<IActionResult> Logout()
        {
            if(signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if(!ModelState.IsValid)
            {
                return View(loginDTO);
            }

            var result = await signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, loginDTO.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid login attempt";
            }

            return View(loginDTO);
        }
    }
}
