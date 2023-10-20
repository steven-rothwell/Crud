using Crud.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Crud.Api.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ApplicationOptions _applicationOptions;

        public BaseApiController(IOptions<ApplicationOptions> applicationOptions)
        {
            _applicationOptions = applicationOptions.Value;
        }

        protected virtual IActionResult InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        protected virtual IActionResult InternalServerError(Exception exception)
        {
            if (_applicationOptions.ShowExceptions)
                return StatusCode(StatusCodes.Status500InternalServerError, exception.ToString());

            return InternalServerError();
        }

        protected virtual IActionResult InternalServerError(String? message)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }

        protected virtual IActionResult MethodNotAllowed(String? message)
        {
            return StatusCode(StatusCodes.Status405MethodNotAllowed, message);
        }
    }
}
