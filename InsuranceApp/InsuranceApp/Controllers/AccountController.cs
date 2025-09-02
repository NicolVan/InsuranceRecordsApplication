using InsuranceApp.Data;
using InsuranceApp.Models;
using InsuranceApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace InsuranceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = string.IsNullOrWhiteSpace(model.FullName) ? "User" : model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {

                var insuredPerson = new InsuredPerson
                {
                    FullName = string.IsNullOrWhiteSpace(model.FullName) ? "User" : model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    ApplicationUserId = user.Id,
                    PhoneNumber = model.PhoneNumber,
                    Street = model.Street,
                    City = model.City,
                    PostCode = model.PostCode,
                    Country = model.Country,
                };

                _context.InsuredPersons.Add(insuredPerson);
                await _context.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, "Client");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = token },
                    protocol: HttpContext.Request.Scheme
                );

                var emailBody = $"Hello {user.FullName},<br/>Click on the link to confirm your email: <a class='btn btn-primary' href='{callbackUrl}'>Confirm email</a>";
                await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

                return RedirectToAction("Index", "InsuredPersons");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction("About", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("ConfirmEmailSuccess"); 
            }

            return View("Error");
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                    return RedirectToAction("Index", "InsuredPersons");

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("About", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your password has been changed successfully.";
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Details", "InsuredPersons");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine("User is null");
            }
            else if (!(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Console.WriteLine("User email not confirmed");
            }
            else
            {
                Console.WriteLine("User exists and email confirmed");
            }
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account",
                new { token = encodedToken, email = user.Email }, protocol: HttpContext.Request.Scheme);

            var templatePath = Path.Combine(_env.WebRootPath, "EmailTemplates", "ResetPassword.cshtml");
            var template = await System.IO.File.ReadAllTextAsync(templatePath);

            var emailBody = template.Replace("{{ResetLink}}", callbackUrl);

            await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction("About", "Home");

            var decodedToken = System.Web.HttpUtility.UrlDecode(token);
            return View(new ResetPasswordViewModel { Token = decodedToken, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var decodedToken = System.Web.HttpUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
