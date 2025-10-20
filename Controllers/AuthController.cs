using FYP___Vehicules_Service_and_Maintenance_Record_System.Data;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Models;
using FYP___Vehicules_Service_and_Maintenance_Record_System.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEncryptionService _encryptionService;
        private readonly IEmailService _emailService;

        public AuthController(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            IEncryptionService encryptionService,
            IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _encryptionService = encryptionService;
            _emailService = emailService;
        }

        // GET: Auth/SelectRole (Main Menu)
        [HttpGet]
        public IActionResult SelectRole()
        {
            return View("~/Views/Auth/MainMenu.cshtml");
        }

        // GET: Auth/CustomerLogin
        [HttpGet]
        public IActionResult CustomerLogin()
        {
            return View("~/Views/Auth/CustomerLogin.cshtml");
        }

        // POST: Auth/CustomerLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerLogin(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("~/Views/Auth/CustomerLogin.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Customer)
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 3);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/CustomerLogin.cshtml");
                }

                if (!_passwordHasher.VerifyPassword(Password, user.Password))
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/CustomerLogin.cshtml");
                }

                await SignInUser(user, RememberMe);

                TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
                return View("~/Views/Auth/CustomerLogin.cshtml");
            }
        }

        // GET: Auth/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }

        // POST: Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                ViewBag.ErrorMessage = "Email is required.";
                return View("~/Views/Auth/ForgotPassword.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 3);

                if (user == null)
                {
                    TempData["SuccessMessage"] = "If an account exists with this email, a reset code has been sent.";
                    return View("~/Views/Auth/ForgotPassword.cshtml");
                }

                // Generate 6-digit reset code
                var resetCode = GenerateResetCode();

                // Save reset code to database
                var passwordReset = new PasswordReset
                {
                    Email = encryptedEmail,
                    ResetCode = resetCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                };

                _context.PasswordResets.Add(passwordReset);
                await _context.SaveChangesAsync();

                // Send reset code via email
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(
                        Email,
                        $"{user.FirstName} {user.LastName}",
                        resetCode
                    );
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Failed to send email. Please try again later.";
                    return View("~/Views/Auth/ForgotPassword.cshtml");
                }

                return RedirectToAction("ResetPassword", new { email = Email });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again.";
                return View("~/Views/Auth/ForgotPassword.cshtml");
            }
        }

        // GET: Auth/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = email;
            return View("~/Views/Auth/ResetPassword.cshtml");
        }

        // POST: Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            string Email,
            string ResetCode,
            string NewPassword,
            string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(ResetCode) ||
                string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            if (NewPassword.Length < 6)
            {
                ViewBag.ErrorMessage = "Password must be at least 6 characters long.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var resetRecord = await _context.PasswordResets
                    .Where(r => r.Email == encryptedEmail && r.ResetCode == ResetCode && !r.IsUsed)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (resetRecord == null)
                {
                    ViewBag.ErrorMessage = "Invalid reset code.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/ResetPassword.cshtml");
                }

                if (DateTime.UtcNow > resetRecord.ExpiresAt)
                {
                    ViewBag.ErrorMessage = "Reset code has expired. Please request a new one.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/ResetPassword.cshtml");
                }

                resetRecord.IsUsed = true;
                await _context.SaveChangesAsync();

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 3);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/ResetPassword.cshtml");
                }

                user.Password = _passwordHasher.HashPassword(NewPassword);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password reset successful! You can now login with your new password.";
                return RedirectToAction("CustomerLogin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }
        }

        // Helper method to generate 6-digit reset code
        private string GenerateResetCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // GET: Auth/CustomerRegister
        [HttpGet]
        public IActionResult CustomerRegister()
        {
            return View("~/Views/Auth/CustomerRegister.cshtml");
        }

        // POST: Auth/CustomerRegister
        // POST: Auth/CustomerRegister
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegister(
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Address,
            string Password,
            string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) ||
                string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(PhoneNumber))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View("~/Views/Auth/CustomerRegister.cshtml");
            }

            if (Password != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View("~/Views/Auth/CustomerRegister.cshtml");
            }

            if (Password.Length < 6)
            {
                ViewBag.ErrorMessage = "Password must be at least 6 characters long.";
                return View("~/Views/Auth/CustomerRegister.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail);

                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "An account with this email already exists.";
                    return View("~/Views/Auth/CustomerRegister.cshtml");
                }

                // Generate 6-digit OTP
                var otpCode = GenerateResetCode();

                // Hash password
                var hashedPassword = _passwordHasher.HashPassword(Password);

                // Save registration data temporarily
                var registrationVerification = new RegistrationVerification
                {
                    Email = encryptedEmail,
                    OtpCode = otpCode,
                    FirstName = FirstName,
                    LastName = LastName,
                    PhoneNumber = PhoneNumber,
                    Address = Address,
                    HashedPassword = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsVerified = false
                };

                _context.RegistrationVerifications.Add(registrationVerification);
                await _context.SaveChangesAsync();

                // Send OTP email
                try
                {
                    await _emailService.SendRegistrationOtpEmailAsync(
                        Email,
                        $"{FirstName} {LastName}",
                        otpCode
                    );
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Failed to send verification email. Please try again.";
                    return View("~/Views/Auth/CustomerRegister.cshtml");
                }

                // Redirect to verification page
                return RedirectToAction("VerifyRegistrationOtp", new { email = Email });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during registration. Please try again.";
                return View("~/Views/Auth/CustomerRegister.cshtml");
            }
        }

        // GET: Auth/VerifyRegistrationOtp
        [HttpGet]
        public IActionResult VerifyRegistrationOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("CustomerRegister");
            }

            ViewBag.Email = email;
            return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
        }

        // POST: Auth/VerifyRegistrationOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyRegistrationOtp(string Email, string OtpCode)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(OtpCode))
            {
                ViewBag.ErrorMessage = "Email and OTP code are required.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                // Find the registration verification record
                var verificationRecord = await _context.RegistrationVerifications
                    .Where(r => r.Email == encryptedEmail && r.OtpCode == OtpCode && !r.IsVerified)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationRecord == null)
                {
                    ViewBag.ErrorMessage = "Invalid verification code.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
                }

                // Check if OTP is expired
                if (DateTime.UtcNow > verificationRecord.ExpiresAt)
                {
                    ViewBag.ErrorMessage = "Verification code has expired. Please request a new one.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
                }

                // Mark as verified
                verificationRecord.IsVerified = true;
                await _context.SaveChangesAsync();

                // Create the user account
                var user = new User
                {
                    RoleID = 3,
                    FirstName = verificationRecord.FirstName,
                    LastName = verificationRecord.LastName,
                    Email = encryptedEmail,
                    Password = verificationRecord.HashedPassword,
                    PhoneNumber = verificationRecord.PhoneNumber
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create customer profile
                var customer = new Customer
                {
                    UserID = user.ID,
                    Address = verificationRecord.Address
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Email verified successfully! You can now login.";
                return RedirectToAction("CustomerLogin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred. Please try again.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
            }
        }

        // POST: Auth/ResendRegistrationOtp
        // POST: Auth/ResendRegistrationOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendRegistrationOtp(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return RedirectToAction("CustomerRegister");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                // Find the latest registration verification
                var verificationRecord = await _context.RegistrationVerifications
                    .Where(r => r.Email == encryptedEmail && !r.IsVerified)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (verificationRecord == null)
                {
                    ViewBag.ErrorMessage = "Registration session not found. Please register again.";
                    return RedirectToAction("CustomerRegister");
                }

                // Generate NEW OTP
                var newOtpCode = GenerateResetCode();

                // Update the record with NEW OTP
                verificationRecord.OtpCode = newOtpCode;
                verificationRecord.CreatedAt = DateTime.UtcNow;
                verificationRecord.ExpiresAt = DateTime.UtcNow.AddMinutes(10);

                await _context.SaveChangesAsync();

                // IMPORTANT: Send the NEW OTP (not the old one)
                try
                {
                    await _emailService.SendRegistrationOtpEmailAsync(
                        Email,
                        $"{verificationRecord.FirstName} {verificationRecord.LastName}",
                        newOtpCode  // Use newOtpCode instead of otpCode
                    );
                }
                catch (Exception emailEx)
                {
                    ViewBag.ErrorMessage = "Failed to send verification email. Please try again.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
                }

                TempData["SuccessMessage"] = "A new verification code has been sent to your email.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed to resend verification code. Please try again.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/VerifyRegistrationOtp.cshtml");
            }
        }

        // GET: Auth/EmployeeLogin
        [HttpGet]
        public IActionResult EmployeeLogin()
        {
            return View("~/Views/Auth/EmployeeLogin.cshtml");
        }

        // POST: Auth/EmployeeLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeLogin(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("~/Views/Auth/EmployeeLogin.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 2);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/EmployeeLogin.cshtml");
                }

                if (!_passwordHasher.VerifyPassword(Password, user.Password))
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/EmployeeLogin.cshtml");
                }

                await SignInUser(user, RememberMe);

                TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
                return RedirectToAction("Index", "Employee");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
                return View("~/Views/Auth/EmployeeLogin.cshtml");
            }
        }

        // GET: Auth/AdminLogin
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View("~/Views/Auth/AdminLogin.cshtml");
        }

        // POST: Auth/AdminLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Email and password are required.";
                return View("~/Views/Auth/AdminLogin.cshtml");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Admin)
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 1);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/AdminLogin.cshtml");
                }

                if (!_passwordHasher.VerifyPassword(Password, user.Password))
                {
                    ViewBag.ErrorMessage = "Invalid email or password.";
                    return View("~/Views/Auth/AdminLogin.cshtml");
                }

                await SignInUser(user, RememberMe);

                TempData["SuccessMessage"] = $"Welcome back, {user.FirstName}!";
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
                return View("~/Views/Auth/AdminLogin.cshtml");
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("SelectRole");
        }

        // Helper method to sign in user
        private async Task SignInUser(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Email, _encryptionService.Decrypt(user.Email)),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("RoleID", user.RoleID.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);
        }

        // POST: Auth/ResendPasswordResetCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendPasswordResetCode(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return RedirectToAction("ForgotPassword");
            }

            try
            {
                var encryptedEmail = _encryptionService.Encrypt(Email);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.RoleID == 3);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/ResetPassword.cshtml");
                }

                // Generate NEW reset code
                var newResetCode = GenerateResetCode();

                // Create new password reset record
                var passwordReset = new PasswordReset
                {
                    Email = encryptedEmail,
                    ResetCode = newResetCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                };

                _context.PasswordResets.Add(passwordReset);
                await _context.SaveChangesAsync();

                // Send NEW reset code via email
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(
                        Email,
                        $"{user.FirstName} {user.LastName}",
                        newResetCode
                    );
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Failed to send email. Please try again.";
                    ViewBag.Email = Email;
                    return View("~/Views/Auth/ResetPassword.cshtml");
                }

                TempData["SuccessMessage"] = "A new reset code has been sent to your email.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed to resend reset code. Please try again.";
                ViewBag.Email = Email;
                return View("~/Views/Auth/ResetPassword.cshtml");
            }
        }
    }
}