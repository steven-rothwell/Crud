using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Controllers;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.Services;
using Crud.Api.Tests.TestingModels;
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
        private Mock<IQueryCollectionService> _queryCollectionService;
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
            _queryCollectionService = new Mock<IQueryCollectionService>();
            _stream = new MemoryStream(Encoding.UTF8.GetBytes("this-does-not-matter"));
            var httpContext = new DefaultHttpContext() { Request = { Body = _stream, ContentLength = _stream.Length } };
            var controllerContext = new ControllerContext { HttpContext = httpContext };

            _controller = new CrudController(_applicationOptions, _logger.Object, _validator.Object, _preserver.Object, _streamService.Object, _typeService.Object, _queryCollectionService.Object) { ControllerContext = controllerContext };
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
            Type? type = typeof(Model);

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
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(model.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<Model>())).ReturnsAsync(validationResult);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task CreateAsync_ModelCreated_ReturnsOkCreatedModel()
        {
            var typeName = "some-type-name";
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(model.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.CreateAsync(It.IsAny<Model>())).ReturnsAsync(model);

            var result = await _controller.CreateAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as Model;

            Assert.NotNull(typedResult);
            Assert.Equal(model.Id, typedResult.Id);
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
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            Model model = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(model);

            var result = await _controller.ReadAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundRead, typeName), result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ModelIsFound_ReturnsFoundModel()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(model);

            var result = await _controller.ReadAsync(typeName, id) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as Model;

            Assert.NotNull(typedResult);
            Assert.Equal(model.Id, typedResult.Id);
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

        #region ReadAsync_WithString
        [Fact]
        public async Task ReadAsync_WithString_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.ReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithString_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _validator.Setup(m => m.ValidateReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(validationResult);

            var result = await _controller.ReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithString_ModelsAreFound_ReturnsFoundModels()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var models = new List<Model> { new Model { Id = 1 } };
            var validationResult = new ValidationResult { IsValid = true };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _validator.Setup(m => m.ValidateReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<IDictionary<string, string>>())).ReturnsAsync(models);

            var result = await _controller.ReadAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as IEnumerable<Model>;

            Assert.NotNull(typedResult);
            Assert.Equal(models.FirstOrDefault()!.Id, typedResult.FirstOrDefault()?.Id);
        }

        [Fact]
        public async Task ReadAsync_WithString_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.ReadAsync(typeName) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.UpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task UpdateAsync_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.UpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task UpdateAsync_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>())).ReturnsAsync(validationResult);

            var result = await _controller.UpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task UpdateAsync_UpdatedModelIsNull_ReturnsNotFound()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            Model updatedModel = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>())).ReturnsAsync(updatedModel);

            var result = await _controller.UpdateAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundUpdate, typeName), result.Value);
        }

        [Fact]
        public async Task UpdateAsync_UpdatedModelIsFound_ReturnsUpdatedModel()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            Model updatedModel = model;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>())).ReturnsAsync(updatedModel);

            var result = await _controller.UpdateAsync(typeName, id) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as Model;

            Assert.NotNull(typedResult);
            Assert.Equal(model.Id, typedResult.Id);
        }

        [Fact]
        public async Task UpdateAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Guid id = Guid.Empty;
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.UpdateAsync(typeName, id) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region PartialUpdateAsync_WithStringGuid
        [Fact]
        public async Task PartialUpdateAsync_WithStringGuid_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.PartialUpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PartialUpdateAsync_WithStringGuid_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.PartialUpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithStringGuid_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);

            var result = await _controller.PartialUpdateAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithStringGuid_UpdatedModelIsNull_ReturnsNotFound()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            Model updatedModel = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.PartialUpdateAsync<Model>(It.IsAny<Guid>(), It.IsAny<IDictionary<String, JsonNode>>())).ReturnsAsync(updatedModel);

            var result = await _controller.PartialUpdateAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundUpdate, typeName), result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithStringGuid_UpdatedModelIsFound_ReturnsUpdatedModel()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            Model updatedModel = model;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.PartialUpdateAsync<Model>(It.IsAny<Guid>(), It.IsAny<IDictionary<String, JsonNode>>())).ReturnsAsync(updatedModel);

            var result = await _controller.PartialUpdateAsync(typeName, id) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as Model;

            Assert.NotNull(typedResult);
            Assert.Equal(model.Id, typedResult.Id);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithStringGuid_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Guid id = Guid.Empty;
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.PartialUpdateAsync(typeName, id) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion
    }
}
