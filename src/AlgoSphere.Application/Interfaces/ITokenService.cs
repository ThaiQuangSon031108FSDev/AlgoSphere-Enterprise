namespace AlgoSphere.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(int userId, string username, string email, IEnumerable<string> roles);
}
