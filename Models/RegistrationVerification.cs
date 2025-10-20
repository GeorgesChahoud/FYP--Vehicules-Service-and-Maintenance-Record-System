namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class RegistrationVerification
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string HashedPassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; }
    }
}