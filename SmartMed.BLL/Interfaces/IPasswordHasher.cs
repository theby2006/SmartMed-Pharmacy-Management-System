namespace SmartMed.BLL.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password, string salt);

        bool VerifyPassword(string password, string hash, string salt);

        string GenerateSalt();
    }
}
