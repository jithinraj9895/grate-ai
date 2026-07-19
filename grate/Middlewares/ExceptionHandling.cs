using System.Diagnostics;
using System.Net;
using System.Text.Json;

public class ExceptionHandling
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandling> _logger;

    public ExceptionHandling(RequestDelegate requestDelegate, ILogger<ExceptionHandling> logger)
    {
        _next = requestDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("There is an error ::::" + httpContext.User.Claims.FirstOrDefault());
            _logger.LogError(ex.Message);

            await HandleExcdeptionAsync(httpContext, ex);
        }

    }


    public async Task HandleExcdeptionAsync(HttpContext context, Exception ex)
    {
        var response = new ErrorResponseDto();

        switch (ex)
        {
            case NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusCode = context.Response.StatusCode;
                response.Message = ex.Message;
                break;
            case BadRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = context.Response.StatusCode;
                response.Message = ex.Message;
                break;
            case UnAuthorizedUserException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.StatusCode = context.Response.StatusCode;
                response.Message = ex.Message;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode = context.Response.StatusCode;
                response.Message = ex.Message + "Don't know what is happening";
                break;
        }
        var res = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(res);
    }

}

