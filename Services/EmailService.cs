using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;

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

        public async Task SendAppointmentConfirmationEmailAsync(
            string recipientEmail,
            string recipientName,
            string serviceName,
            DateTime appointmentDateTime,
            string carDetails,
            string specialRequest = null)
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
                message.Subject = "CarHub - Appointment Confirmation";

                // Create the calendar event (iCalendar format)
                var icsContent = GenerateICalendarEvent(
                    recipientName,
                    recipientEmail,
                    serviceName,
                    appointmentDateTime,
                    carDetails,
                    specialRequest
                );

                var builder = new BodyBuilder();
                
                // HTML Body
                builder.HtmlBody = $@"
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
                            .appointment-box {{ background-color: #f8f9fa; border-left: 4px solid #FF0000; padding: 20px; margin: 30px 0; }}
                            .appointment-box h3 {{ margin-top: 0; color: #FF0000; }}
                            .detail-row {{ display: flex; padding: 10px 0; border-bottom: 1px solid #e9ecef; }}
                            .detail-label {{ font-weight: bold; color: #6c757d; width: 150px; }}
                            .detail-value {{ color: #333; }}
                            .calendar-info {{ background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
                            .calendar-info p {{ margin: 0; color: #155724; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 12px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Appointment Confirmed!</h1>
                            </div>
                            <div class='content'>
                                <p>Hello <strong>{recipientName}</strong>,</p>
                                <p>Your appointment at <strong>CarHub Garage</strong> has been successfully booked!</p>
                                
                                <div class='appointment-box'>
                                    <h3>📋 Appointment Details</h3>
                                    <div class='detail-row'>
                                        <span class='detail-label'>Service:</span>
                                        <span class='detail-value'>{serviceName}</span>
                                    </div>
                                    <div class='detail-row'>
                                        <span class='detail-label'>Date & Time:</span>
                                        <span class='detail-value'>{appointmentDateTime.ToString("dddd, MMMM dd, yyyy 'at' hh:mm tt")}</span>
                                    </div>
                                    <div class='detail-row'>
                                        <span class='detail-label'>Vehicle:</span>
                                        <span class='detail-value'>{carDetails}</span>
                                    </div>
                                    {(string.IsNullOrEmpty(specialRequest) ? "" : $@"
                                    <div class='detail-row'>
                                        <span class='detail-label'>Special Request:</span>
                                        <span class='detail-value'>{specialRequest}</span>
                                    </div>")}
                                </div>

                                <div class='calendar-info'>
                                    <p><strong>📅 Calendar Invitation Attached!</strong></p>
                                    <p>A calendar event (.ics file) is attached to this email. Click on it to add this appointment to your Google Calendar, Outlook, or any other calendar app.</p>
                                </div>

                                <p><strong>Location:</strong> CarHub Garage<br>123 Street, Beirut, Lebanon</p>
                                
                                <p><strong>What to bring:</strong></p>
                                <ul>
                                    <li>Your vehicle registration documents</li>
                                    <li>Previous service records (if available)</li>
                                </ul>

                                <p>If you need to cancel or reschedule, please visit your appointments page or contact us at <strong>+961 03 866 298</strong></p>
                                
                                <p style='margin-top: 30px;'>We look forward to seeing you!<br><strong>CarHub Garage Team</strong></p>
                            </div>
                            <div class='footer'>
                                <p>This is an automated confirmation from CarHub Garage</p>
                                <p>123 Street, Beirut, Lebanon | +961 03 866 298</p>
                                <p>&copy; 2025 CarHub Garage. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                // Attach the .ics calendar file
                var icsBytes = Encoding.UTF8.GetBytes(icsContent);
                builder.Attachments.Add("appointment.ics", icsBytes, new ContentType("text", "calendar"));

                message.Body = builder.ToMessageBody();

                await SendEmailAsync(message);
                _logger.LogInformation($"Appointment confirmation email sent to {recipientEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send appointment confirmation email: {ex.Message}");
                throw new InvalidOperationException($"Failed to send appointment confirmation email: {ex.Message}", ex);
            }
        }

        // Generate iCalendar (.ics) format for calendar apps
        private string GenerateICalendarEvent(
            string recipientName,
            string recipientEmail,
            string serviceName,
            DateTime appointmentDateTime,
            string carDetails,
            string specialRequest)
        {
            var startTime = appointmentDateTime;
            var endTime = appointmentDateTime.AddHours(1); // Assume 1 hour duration

            // Format dates in UTC for iCalendar (YYYYMMDDTHHMMSSZ)
            var startTimeUtc = startTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            var endTimeUtc = endTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            var dateStamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
            var uid = Guid.NewGuid().ToString();

            var description = $"Service: {serviceName}\\nVehicle: {carDetails}";
            if (!string.IsNullOrEmpty(specialRequest))
            {
                description += $"\\nSpecial Request: {specialRequest}";
            }

            var icsContent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//CarHub Garage//Appointment System//EN
CALSCALE:GREGORIAN
METHOD:REQUEST
BEGIN:VEVENT
UID:{uid}
DTSTAMP:{dateStamp}
DTSTART:{startTimeUtc}
DTEND:{endTimeUtc}
SUMMARY:CarHub - {serviceName}
DESCRIPTION:{description}
LOCATION:CarHub Garage, 123 Street, Beirut, Lebanon
ORGANIZER;CN=CarHub Garage:mailto:carhubgarage@gmail.com
ATTENDEE;CN={recipientName};RSVP=TRUE:mailto:{recipientEmail}
STATUS:CONFIRMED
SEQUENCE:0
BEGIN:VALARM
TRIGGER:-PT1H
ACTION:DISPLAY
DESCRIPTION:Appointment Reminder - CarHub Garage in 1 hour
END:VALARM
BEGIN:VALARM
TRIGGER:-PT24H
ACTION:DISPLAY
DESCRIPTION:Appointment Reminder - CarHub Garage tomorrow
END:VALARM
END:VEVENT
END:VCALENDAR";

            return icsContent;
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