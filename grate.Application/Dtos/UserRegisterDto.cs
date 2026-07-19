public class UserRegisterDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }   // plain password from client
    public string? PhoneNumber { get; set; }
}


public class UserLoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }   // plain password from client
}