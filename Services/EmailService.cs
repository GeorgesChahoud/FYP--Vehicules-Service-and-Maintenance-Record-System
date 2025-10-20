using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetCode)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailSettings["SenderName"],
                emailSettings["SenderEmail"]
            ));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "CarHub - Password Reset Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
                            .header {{ background-color: #FF0000; color: white; padding: 30px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                            .code-box {{ background-color: #f8f9fa; border-left: 4px solid #FF0000; padding: 20px; margin: 30px 0; text-align: center; }}
                            .code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #FF0000; font-family: 'Courier New', monospace; }}
                            .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                            .warning p {{ margin: 0; color: #856404; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Password Reset Request</h1>
                            </div>
                            <div class='content'>
                                <p>Hello <strong>{recipientName}</strong>,</p>
                                <p>We received a request to reset your password for your CarHub account. Use the code below to reset your password:</p>
                                
                                <div class='code-box'>
                                    <div class='code'>{resetCode}</div>
                                </div>

                                <div class='warning'>
                                    <p><strong>This code expires in 15 minutes</strong></p>
                                </div>

                                <p>Enter this code on the password reset page to create a new password.</p>
                                
                                <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
                                
                                <p style='margin-top: 30px;'>Best regards,<br><strong>CarHub Garage Team</strong></p>
                            </div>
                            <div class='footer'>
                                <p>This is an automated message from CarHub Garage</p>
                                <p>Please do not reply to this email</p>
                                <p>&copy; 2025 CarHub Garage. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password reset email failed: {ex.Message}");
                throw new InvalidOperationException($"Failed to send email: {ex.Message}");
            }
        }

        public async Task SendRegistrationOtpEmailAsync(string recipientEmail, string recipientName, string otpCode)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]
                ));
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                message.Subject = "CarHub - Verify Your Email Address";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                                .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
                                .header {{ background-color: #FF0000; color: white; padding: 30px; text-align: center; }}
                                .header h1 {{ margin: 0; font-size: 28px; }}
                                .content {{ padding: 40px 30px; }}
                                .content p {{ color: #333; line-height: 1.6; font-size: 16px; }}
                                .code-box {{ background-color: #f8f9fa; border-left: 4px solid #FF0000; padding: 20px; margin: 30px 0; text-align: center; }}
                                .code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #FF0000; font-family: 'Courier New', monospace; }}
                                .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                                .warning p {{ margin: 0; color: #856404; }}
                                .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 12px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>Welcome to CarHub!</h1>
                                </div>
                                <div class='content'>
                                    <p>Hello <strong>{recipientName}</strong>,</p>
                                    <p>Thank you for registering with CarHub Garage! To complete your registration, please verify your email address using the code below:</p>
                                    
                                    <div class='code-box'>
                                        <div class='code'>{otpCode}</div>
                                    </div>

                                    <div class='warning'>
                                        <p><strong>This code expires in 10 minutes</strong></p>
                                    </div>

                                    <p>Enter this code on the verification page to activate your account and start booking our services!</p>
                                    
                                    <p>If you didn't create an account with CarHub, please ignore this email.</p>
                                    
                                    <p style='margin-top: 30px;'>Best regards,<br><strong>CarHub Garage Team</strong></p>
                                </div>
                                <div class='footer'>
                                    <p>This is an automated message from CarHub Garage</p>
                                    <p>Please do not reply to this email</p>
                                    <p>&copy; 2025 CarHub Garage. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();
                await SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration email sending failed: {ex.Message}");
                throw new InvalidOperationException($"Failed to send registration email: {ex.Message}", ex);
            }
        }

        // Private helper method to send emails
        private async Task SendEmailAsync(MimeMessage message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            using var client = new SmtpClient();
            try
            {
                _logger.LogInformation("Attempting to connect to SMTP server...");

                await client.ConnectAsync(
                    emailSettings["SmtpServer"],
                    int.Parse(emailSettings["SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                _logger.LogInformation("Connected. Authenticating...");

                await client.AuthenticateAsync(
                    emailSettings["Username"],
                    emailSettings["Password"]
                );

                _logger.LogInformation("Authenticated. Sending email...");

                await client.SendAsync(message);

                _logger.LogInformation("Email sent successfully!");

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"SMTP Error: {ex.Message}");
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}