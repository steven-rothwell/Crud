using System.Text;
using System.Text.Json;
using Crud.Api.Constants;
using Crud.Api.Controllers;
using Crud.Api.Models;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.Services;
using Crud.Api.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Crud.Api.Tests.Controllers
{
    public class CrudControllerTests : IDisposable
    {
        private IOptions<ApplicationOptions> _applicationOptions;
        private Mock<ILogger<CrudController>> _logger;
        private Mock<IValidator> _validator;
        private Mock<IPreserver> _preserver;
        private Mock<IStreamService> _streamService;
        private Mock<ITypeService> _typeService;
        private CrudController _controller;
        private Stream _stream;

        public CrudControllerTests()
        {
            _applicationOptions = Microsoft.Extensions.Options.Options.Create(new ApplicationOptions { ShowExceptions = false });
            _logger = new Mock<ILogger<CrudController>>();
            _validator = new Mock<IValidator>();
            _preserver = new Mock<IPreserver>();
            _streamService = new Mock<IStreamService>();
            _typeService = new Mock<ITypeService>();
            _stream = new MemoryStream(Encoding.UTF8.GetBytes("this-does-not-matter"));
            var httpContext = new DefaultHttpContext() { Request = { Body = _stream, ContentLength = _stream.Length } };
            var controllerContext = new ControllerContext { HttpContext = httpContext };

            _controller = new CrudController(_applicationOptions, _logger.Object, _validator.Object, _preserver.Object, _streamService.Object, _typeService.Object) { ControllerContext = controllerContext };
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateAsync_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(User);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task CreateAsync_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            var user = new User { Age = 1 };
            var json = JsonSerializer.Serialize(user);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(user.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<User>())).ReturnsAsync(validationResult);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task CreateAsync_ModelCreated_ReturnsOkCreatedModel()
        {
            var typeName = "some-type-name";
            var user = new User { Age = 1 };
            var json = JsonSerializer.Serialize(user);
            var validationResult = new ValidationResult
            {
                IsValid = true
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(user.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<User>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.CreateAsync(It.IsAny<User>())).ReturnsAsync(user);

            var result = await _controller.CreateAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as User;

            Assert.NotNull(typedResult);
            Assert.Equal(user.Age, typedResult.Age);
        }

        [Fact]
        public async Task CreateAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.CreateAsync(typeName) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region ReadAsync_WithStringGuid
        [Fact]
        public async Task ReadAsync_WithStringGuid_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.ReadAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ModelIsNull_ReturnsNotFound()
        {
            var typeName = "some-type-name";
            Type? type = typeof(User);
            Guid id = Guid.Empty;
            User user = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.ReadAsync<User>(It.IsAny<Guid>())).ReturnsAsync(user);

            var result = await _controller.ReadAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundRead, typeName), result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ModelIsFound_ReturnsFoundModel()
        {
            var typeName = "some-type-name";
            Type? type = typeof(User);
            Guid id = Guid.Empty;
            User user = new User { Age = 1 };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.ReadAsync<User>(It.IsAny<Guid>())).ReturnsAsync(user);

            var result = await _controller.ReadAsync(typeName, id) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as User;

            Assert.NotNull(typedResult);
            Assert.Equal(user.Age, typedResult.Age);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Guid id = Guid.Empty;
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.ReadAsync(typeName, id) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion
    }
}
