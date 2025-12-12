using System.Text.RegularExpressions;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public class PasswordValidator : IPasswordValidator
    {
        public (bool isValid, string errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password is required.");
            }

            if (password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long.");
            }

            if (password.Length > 128)
            {
                return (false, "Password cannot exceed 128 characters.");
            }

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter (A-Z).");
            }

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter (a-z).");
            }

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "Password must contain at least one number (0-9).");
            }

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[@$!%*?&#^()_+=\[\]{};:'"",.<>\/\\|`~\-]"))
            {
                return (false, "Password must contain at least one special character (@$!%*?&#^()_+=[]{}; etc.).");
            }

            // Check for common weak passwords
            var commonPasswords = new[] { "password", "password123", "admin", "admin123", "12345678", "qwerty", "letmein" };
            if (commonPasswords.Any(cp => password.ToLower().Contains(cp)))
            {
                return (false, "Password contains common weak patterns. Please choose a stronger password.");
            }

            return (true, string.Empty);
        }
    }
}
