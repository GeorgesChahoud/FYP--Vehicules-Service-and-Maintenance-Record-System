namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string recipientEmail, string recipientName, string resetCode);
        Task SendRegistrationOtpEmailAsync(string recipientEmail, string recipientName, string otpCode);
        Task SendAppointmentConfirmationEmailAsync(
            string recipientEmail, 
            string recipientName, 
            string serviceName,
            DateTime appointmentDateTime, 
            string carDetails,
            string specialRequest = null);
    }
}
