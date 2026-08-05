using HRMS.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        
        protected IActionResult ToActionResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return CreateProblemResponse(result.Error);
        }

        
        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Value is null ? NoContent() : Ok(result.Value);
            }

            return CreateProblemResponse(result.Error);
        }

        private IActionResult CreateProblemResponse(Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForErrorType(error.Type),
                Detail = error.Message,
                Instance = HttpContext.Request.Path
            };

            problemDetails.Extensions["errorCode"] = error.Code;

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode,
                ContentTypes = { "application/problem+json" }
            };
        }

        private static string GetTitleForErrorType(ErrorType type) => type switch
        {
            ErrorType.Validation => "Validation Error",
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Conflict => "Conflict Error",
            ErrorType.Unauthorized => "Unauthorized Access",
            ErrorType.Forbidden => "Forbidden Access",
            _ => "Server Error"
        };
    }
}
