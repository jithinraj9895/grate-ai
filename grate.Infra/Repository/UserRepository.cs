using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> GetUserData(UserLoginDto userLoginDto)
    {
        var user = await _context.Users.SingleAsync(x => x.Email == userLoginDto.Email);
        return user;
    }
}