using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechNest.Models;
using TechNest.Services;

namespace TechNest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
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
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
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

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");

            }

            var profileDTO = new ProfileDTO()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? "",
                PhoneNumber = appUser.PhoneNumber,
                Address = appUser.Address
            };

            return View(profileDTO);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDTO profileDTO)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all the required feilds with valid values";
                return View(profileDTO);
            }

            //get the current authenticated user
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //update the user profile
            appUser.FirstName = profileDTO.FirstName;
            appUser.LastName = profileDTO.LastName;
            appUser.UserName = profileDTO.Email;
            appUser.Email = profileDTO.Email;
            appUser.PhoneNumber = profileDTO.PhoneNumber;
            appUser.Address = profileDTO.Address;

            var result = await userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                ViewBag.SuccessMessage = "Profile updated successfully";
            }
            else
            {
                ViewBag.ErrorMessage = "Failed to update the profile: " + result.Errors.First().Description;
            }
            
            return View(profileDTO);
        }

        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Password(PasswordDTO passwordDTO)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //get the current authenticated user
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //update the user password
            var result = await userManager.ChangePasswordAsync(appUser, passwordDTO.CurrentPassword, passwordDTO.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password updated successfully";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Failed to update the password: " + result.Errors.First().Description;
            }

            return View();
        }

        public IActionResult ForgotPassword()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Required, EmailAddress] string email)
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.EmailError = ModelState["email"]?.Errors.First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                //generate password reset token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                string resultUrl = Url.ActionLink("ResetPassword", "Account", new { token }) ?? "URL Error";

                //send url by email
                string senderName = configuration["BrevoSettings:SenderName"] ?? "";
                string senderEmail = configuration["BrevoSettings:SenderEmail"] ?? "";
                string username = user.FirstName + " " + user.LastName;
                string subject = "Password Reset";
                string message = "Dear " + username + ",\n\n" +
                    "Please click the link below to reset your password:\n" +
                    resultUrl + "\n\n" +
                    "If you did not request a password reset, please ignore this email.\n\n" +
                    "Regards,\n" +
                    "Brevo Team";

                EmailSender.SendEmail(senderName, senderEmail, username, email, subject, message);
            }

            ViewBag.SuccessMessage = "If the email address is registered, a password reset link will be sent to the email address";

            return View();
        }

        public IActionResult AccessDenied()
        {
            TempData["AccessDeniedMessage"] = "Only admin has access to the page you've requested";
            
            return RedirectToAction("Index", "Home");
        }
    }
}
