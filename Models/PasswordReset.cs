namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Models
{
    public class PasswordReset
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string ResetCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}