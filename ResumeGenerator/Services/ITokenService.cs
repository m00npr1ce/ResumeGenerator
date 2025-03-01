namespace ResumeGenerator.Services
{
    public interface ITokenService
    {
        string GenerateToken(string email);
    }
}
