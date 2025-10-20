namespace FYP___Vehicules_Service_and_Maintenance_Record_System.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
