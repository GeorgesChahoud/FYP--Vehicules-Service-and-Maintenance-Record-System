namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedpassword);
    }
}
