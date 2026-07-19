using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
public class UserServices(
IUserRepository userRepository,
IUnitOfWork unitOfWork,
IPasswordHasher<User> passwordHasher,
ILogger<UserServices> logger,
IConfiguration configuration
) : IUserServices
{
    public async Task<User> CreateUserService(UserRegisterDto userRegisterDto)
    {
        if (userRegisterDto.FullName == "" || userRegisterDto.FullName is null)
        {
            throw new NotFoundException("Username is not found (-.-)");
        }
        var newUser = new User
        {
            FullName = userRegisterDto.FullName,
            PasswordHash = userRegisterDto.Password,
            Email = userRegisterDto.Email,
            PhoneNumber = userRegisterDto.PhoneNumber
        };

        newUser.PasswordHash = passwordHasher.HashPassword(newUser, newUser.PasswordHash);

        var user = await userRepository.CreateUserAsync(newUser);
        await unitOfWork.SaveChangesAsync();
        return user;
    }

    public string GenerateJWTToken(User user)
    {
        var claims = new List<Claim>
        {
               new Claim(ClaimTypes.Email,user.Email),
               new Claim(ClaimTypes.Name,user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds

        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<List<User>> getAll()
    {
        return await userRepository.GetAllUsers();
    }

    public async Task<string> LoginService(UserLoginDto userLogin)
    {
        var user = await userRepository.GetUserData(userLogin);
        if (user.FullName == "") throw new NotFoundException("user fullname is empty");
        var password = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLogin.Password);
        if (password == PasswordVerificationResult.Failed)
        {
            throw new UnAuthorizedUserException("user not authenticated");
        }

        string token = GenerateJWTToken(user);
        logger.LogInformation("THE PASSWORD IS CORRECT !!!");
        Console.WriteLine("login success");

        return token;
    }
}