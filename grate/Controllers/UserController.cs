using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserServices userServices, DataSeedRepository dataSeedRepository) : ControllerBase
{
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok("This is a health check");
    }


    [HttpPost("register")]
    public async Task<ActionResult<User>> CreateUser([FromBody] UserRegisterDto user)
    {
        return await userServices.CreateUserService(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult> LoginUser([FromBody] UserLoginDto loginDto)
    {
        return Ok(await userServices.LoginService(loginDto));
    }


    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> getAll()
    {
        return Ok(await userServices.getAll());
    }

    [HttpGet("error")]
    public async Task<IActionResult> testError()
    {
        throw new Exception("error testing middleware");
        return Ok(await userServices.getAll());
    }


    [HttpGet("cstm")]
    public IActionResult Custom()
    {
        var res = new
        {
            status = "Hey this is custome",
            top = "the topper",
            how_hard = "good",
            not_ai = "yes"
        };
        return Ok(res);
    }

    [HttpGet("seed")]
    public async Task<IActionResult> seeding([FromQuery] int userCount, [FromQuery] int productPerUser)
    {
        await dataSeedRepository.SeedAsync(userCount, productPerUser);
        return Ok("all done");
    }


    [HttpGet("rseed")]
    public async Task<IActionResult> seedingreview()
    {
        await dataSeedRepository.SeedReviewsAsync(30, 100);
        return Ok("all reviews done");
    }

}