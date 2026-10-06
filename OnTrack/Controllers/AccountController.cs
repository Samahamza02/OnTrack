using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Models;
using OnTrack.ViewModels;
using OnTrack.Services;


namespace OnTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // ---------- Register ----------

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Email confirmation is required before login (see Program.cs:
            // options.SignIn.RequireConfirmedAccount = true), so we do NOT
            // sign the user in here - they must confirm first.
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmLink = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token }, Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email!,
                    "Confirm your RailGo account",
                    $"<h2>Welcome to RailGo</h2><p>Please confirm your account by clicking <a href='{confirmLink}'>here</a>.</p>");
            }
            catch (Exception ex)
            {
                // Don't let a failed email send hide the fact that the
                // account was already created successfully above.
                TempData["Error_Notification"] = "Account created, but the confirmation email could not be sent: " + ex.Message;
            }

            TempData["Successful_Notification"] = "Account created! Please check your email to confirm your account before logging in.";
            return RedirectToAction(nameof(Login));
        }

        // ---------- Confirm Email ----------

        // GET: /Account/ConfirmEmail?userId=...&token=...
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.Succeeded = result.Succeeded;
            return View();
        }

        // ---------- Resend Email Confirmation ----------

        // GET: /Account/ResendEmailConfirmation
        [HttpGet]
        public IActionResult ResendEmailConfirmation() => View();

        // POST: /Account/ResendEmailConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            // Always show the same confirmation message whether or not the
            // account exists - otherwise this form becomes a way to check
            // which emails are registered (an account enumeration leak).
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("ResendEmailConfirmationSent");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmLink = Url.Action("ConfirmEmail", "Account",
                new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Confirm your RailGo account",
                $"<h2>Confirm your account</h2><p>Click <a href='{confirmLink}'>here</a> to confirm your email.</p>");

            return View("ResendEmailConfirmationSent");
        }

        // ---------- Login ----------

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account has been locked. Try again later.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                ViewBag.ShowResendLink = true;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------- Forget / Reset Password ----------

        // GET: /Account/ForgetPassword
        [HttpGet]
        public IActionResult ForgetPassword() => View();

        // POST: /Account/ForgetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            // Same anti-enumeration pattern as ResendEmailConfirmation: show
            // the same "check your email" result either way.
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("ForgetPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account",
                new { email = user.Email, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset your RailGo password",
                $"<h2>Reset your password</h2><p>Click <a href='{resetLink}'>here</a> to choose a new password.</p>");

            return View("ForgetPasswordConfirmation");
        }

        // GET: /Account/ResetPassword?email=...&token=...
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordVM { Email = email, Token = token });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Same email doesn't exist - still show success to avoid
                // leaking which emails are registered.
                return View("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            return View("ResetPasswordConfirmation");
        }
    }
}
