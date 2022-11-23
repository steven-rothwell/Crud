using Crud.Api.Controllers;
using Crud.Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Crud.Api.Tests.Controllers
{
    public class BaseApiControllerTests
    {
        private IOptions<ApplicationOptions> _applicationOptions;
        private DerivedController _controller;

        public BaseApiControllerTests()
        {
            _applicationOptions = Microsoft.Extensions.Options.Options.Create(new ApplicationOptions());
            _controller = new DerivedController(_applicationOptions);
        }

        [Fact]
        public void InternalServerError_ShowExceptionsIsTrue_Returns500AndException()
        {
            var exception = new Exception("An error occurred.");

            _applicationOptions.Value.ShowExceptions = true;

            var result = _controller.CallInternalServerError(exception) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(exception.ToString(), result.Value);
        }

        [Fact]
        public void InternalServerError_ShowExceptionsIsFalse_Returns500()
        {
            var exception = new Exception("An error occurred.");

            _applicationOptions.Value.ShowExceptions = false;

            var result = _controller.CallInternalServerError(exception) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        private class DerivedController : BaseApiController
        {
            public DerivedController(IOptions<ApplicationOptions> applicationOptions) : base(applicationOptions) { }

            public IActionResult CallInternalServerError(Exception exception)
            {
                return InternalServerError(exception);
            }
        }
    }
}
