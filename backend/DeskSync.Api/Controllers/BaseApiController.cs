using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DeskSync.Api.Controllers;

[ApiController]
[Produces("application/json")] 
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult ErrorResult(List<Error> errors)
    {
        if (errors.Count is 0) //if for some reason the IsError was True and no error in list 
        {
            return Problem();// error Code 500 Internal Server Error
        }

        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: firstError.Description);
    }
}