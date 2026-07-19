using Domain.Entities;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);

    Task<List<User>> GetAllUsers();

    Task<User> GetUserData(UserLoginDto userLoginDto);

}