namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public interface IPasswordValidator
    {
        (bool isValid, string errorMessage) ValidatePassword(string password);
    }
}
