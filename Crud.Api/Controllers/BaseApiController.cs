using Crud.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Crud.Api.Controllers
{
    public abstract class BaseApiController<Controller> : ControllerBase
    {
        protected readonly SettingOptions _settingOptions;

        public BaseApiController(IOptions<SettingOptions> settingOptions)
        {
            _settingOptions = settingOptions.Value;
        }

        protected virtual StatusCodeResult InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        protected virtual ActionResult InternalServerError(Exception exception)
        {
            if (_settingOptions.ShowExceptions)
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);

            return InternalServerError();
        }
    }
}
