using AutoFinderAI.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AutoFinderAI.Api.Controllers;

/// <summary>Maps the Application-layer Result/Result&lt;T&gt; pattern to ASP.NET Core responses.</summary>
public static class ControllerResultExtensions
{
    public static ActionResult HandleResult(this ControllerBase controller, Result result)
        => result.IsSuccess ? controller.NoContent() : MapError(controller, result.Error!);

    public static ActionResult<T> HandleResult<T>(this ControllerBase controller, Result<T> result)
        => result.IsSuccess ? controller.Ok(result.Value) : MapError(controller, result.Error!);

    private static ObjectResult MapError(ControllerBase controller, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(detail: error.Message, statusCode: statusCode, title: error.Code);
    }
}
