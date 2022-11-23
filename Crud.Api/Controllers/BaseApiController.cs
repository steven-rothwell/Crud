using Crud.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Crud.Api.Controllers
{
    public abstract class BaseApiController<Controller> : ControllerBase
    {
        protected readonly ApplicationOptions _applicationOptions;

        public BaseApiController(IOptions<ApplicationOptions> applicationOptions)
        {
            _applicationOptions = applicationOptions.Value;
        }

        protected virtual StatusCodeResult InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        protected virtual ActionResult InternalServerError(Exception exception)
        {
            if (_applicationOptions.ShowExceptions)
                return StatusCode(StatusCodes.Status500InternalServerError, exception.ToString());

            return InternalServerError();
        }
    }
}
