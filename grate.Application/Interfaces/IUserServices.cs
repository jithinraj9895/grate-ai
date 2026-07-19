using Domain.Entities;

public interface IUserServices
{
    Task<User> CreateUserService(UserRegisterDto userRegisterDto);
    Task<List<User>> getAll();

    Task<string> LoginService(UserLoginDto userLogin);

    string GenerateJWTToken(User user);
}