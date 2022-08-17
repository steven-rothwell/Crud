using Microsoft.AspNetCore.Mvc;

namespace Crud.Api.Controllers
{
    public abstract class BaseApiController<Controller> : ControllerBase
    {
        protected readonly ILogger _logger;

        public BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual StatusCodeResult InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        protected virtual ActionResult InternalServerError(Exception exception)
        {
#if (DEBUG)
            return StatusCode(StatusCodes.Status500InternalServerError, exception);
#else
            return InternalServerError();
#endif
        }
    }
}
