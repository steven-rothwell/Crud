using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Controllers;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.QueryModels;
using Crud.Api.Services;
using Crud.Api.Services.Models;
using Crud.Api.Tests.TestingModels;
using Crud.Api.Validators;
using Humanizer;
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
        private Mock<IPreprocessingService> _preprocessingService;
        private Mock<IPostprocessingService> _postprocessingService;
        private CrudController _controller;
        private Stream _stream;

        public CrudControllerTests()
        {
            _applicationOptions = Microsoft.Extensions.Options.Options.Create(new ApplicationOptions { ShowExceptions = false, ValidateQuery = false });
            _logger = new Mock<ILogger<CrudController>>();
            _validator = new Mock<IValidator>();
            _preserver = new Mock<IPreserver>();
            _streamService = new Mock<IStreamService>();
            _typeService = new Mock<ITypeService>();
            _queryCollectionService = new Mock<IQueryCollectionService>();
            _preprocessingService = new Mock<IPreprocessingService>();
            _postprocessingService = new Mock<IPostprocessingService>();
            _stream = new MemoryStream(Encoding.UTF8.GetBytes("this-does-not-matter"));
            var httpContext = new DefaultHttpContext() { Request = { Body = _stream, ContentLength = _stream.Length } };
            var controllerContext = new ControllerContext { HttpContext = httpContext };

            _controller = new CrudController(_applicationOptions, _logger.Object, _validator.Object, _preserver.Object, _streamService.Object, _typeService.Object, _queryCollectionService.Object, _preprocessingService.Object, _postprocessingService.Object) { ControllerContext = controllerContext };
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
        public async Task CreateAsync_PreprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(false, "preprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(model.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preprocessingService.Setup(m => m.PreprocessCreateAsync(It.IsAny<Model>())).ReturnsAsync(preprocessingMessageResult);

            var result = await _controller.CreateAsync(typeName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(preprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task CreateAsync_PostprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(false, "postprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(model.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.CreateAsync(It.IsAny<Model>())).ReturnsAsync(model);
            _preprocessingService.Setup(m => m.PreprocessCreateAsync(It.IsAny<Model>())).ReturnsAsync(preprocessingMessageResult);
            _postprocessingService.Setup(m => m.PostprocessCreateAsync(It.IsAny<Model>())).ReturnsAsync(postprocessingMessageResult);

            var result = await _controller.CreateAsync(typeName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(postprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task CreateAsync_ModelCreated_ReturnsOkCreatedModel()
        {
            var typeName = "some-type-name";
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(true);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(model.GetType());
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateCreateAsync(It.IsAny<Model>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.CreateAsync(It.IsAny<Model>())).ReturnsAsync(model);
            _preprocessingService.Setup(m => m.PreprocessCreateAsync(It.IsAny<Model>())).ReturnsAsync(preprocessingMessageResult);
            _postprocessingService.Setup(m => m.PostprocessCreateAsync(It.IsAny<Model>())).ReturnsAsync(postprocessingMessageResult);

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
        public async Task ReadAsync_WithStringGuid_PreprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var preprocessingMessageResult = new MessageResult(false, "preprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(preprocessingMessageResult);

            var result = await _controller.ReadAsync(typeName, id) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(preprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ModelIsNull_ReturnsNotFound()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            Model? model = null;
            var preprocessingMessageResult = new MessageResult(true);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(preprocessingMessageResult);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(model);

            var result = await _controller.ReadAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundRead, typeName), result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_PostprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(false, "postprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(preprocessingMessageResult);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(model);
            _postprocessingService.Setup(m => m.PostprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(postprocessingMessageResult);

            var result = await _controller.ReadAsync(typeName, id) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(postprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithStringGuid_ModelIsFound_ReturnsFoundModel()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var model = new Model { Id = 1 };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(true);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(preprocessingMessageResult);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(model);
            _postprocessingService.Setup(m => m.PostprocessReadAsync(It.IsAny<Model>(), It.IsAny<Guid>())).ReturnsAsync(postprocessingMessageResult);

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
        public async Task ReadAsync_WithString_PreprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(false, "preprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _validator.Setup(m => m.ValidateReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(validationResult);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<String, String>>())).ReturnsAsync(preprocessingMessageResult);

            var result = await _controller.ReadAsync(typeName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(preprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithString_PostprocessingIsNotSuccessful_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(false, "postprocessing-failed");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _validator.Setup(m => m.ValidateReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(validationResult);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<String, String>>())).ReturnsAsync(preprocessingMessageResult);
            _postprocessingService.Setup(m => m.PostprocessReadAsync(It.IsAny<IEnumerable<Model>>(), It.IsAny<IDictionary<String, String>>())).ReturnsAsync(postprocessingMessageResult);

            var result = await _controller.ReadAsync(typeName) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Equal(postprocessingMessageResult.Message, result.Value);
        }

        [Fact]
        public async Task ReadAsync_WithString_ModelsAreFound_ReturnsFoundModels()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var models = new List<Model> { new Model { Id = 1 } };
            var validationResult = new ValidationResult { IsValid = true };
            var preprocessingMessageResult = new MessageResult(true);
            var postprocessingMessageResult = new MessageResult(true);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _validator.Setup(m => m.ValidateReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>>())).ReturnsAsync(validationResult);
            _preprocessingService.Setup(m => m.PreprocessReadAsync(It.IsAny<Model>(), It.IsAny<IDictionary<String, String>>())).ReturnsAsync(preprocessingMessageResult);
            _preserver.Setup(m => m.ReadAsync<Model>(It.IsAny<IDictionary<string, string>>())).ReturnsAsync(models);
            _postprocessingService.Setup(m => m.PostprocessReadAsync(It.IsAny<IEnumerable<Model>>(), It.IsAny<IDictionary<String, String>>())).ReturnsAsync(postprocessingMessageResult);

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

        #region QueryReadAsync

        [Fact]
        public async Task QueryReadAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.QueryReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task QueryReadAsync_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task QueryReadAsync_QueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = null;
            var json = JsonSerializer.Serialize(query);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.BadRequestQuery, $"{nameof(Query)} is null."), result.Value);
        }

        [Fact]
        public async Task QueryReadAsync_DeserializeThrowsExceptionQueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var json = @"{ ""OrderBy"": ""1""}";

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Contains(String.Format(ErrorMessage.BadRequestQuery, String.Empty), result.Value.ToString());
            Assert.Contains("OrderBy", result.Value.ToString());
        }

        [Fact]
        public async Task QueryReadAsync_ValidateQueryIsTrueQueryIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = true;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryReadAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.", result.Value);
        }

        [Fact]
        public async Task QueryReadAsync_ValidateQueryIsFalseQueryIsInvalid_ValidateQueryNotCalled()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryReadAsync(typeName);

            _validator.Verify(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>()), Times.Never);
            _preserver.Verify(m => m.QueryReadAsync<Model>(It.Is<Query>(thisQuery => thisQuery.Limit == query.Limit)), Times.Once);
        }

        [Fact]
        public async Task QueryReadAsync_QueryHasIncludes_ReturnObjectWithOnlyPropertiesRequestedInIncludes()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Includes = new HashSet<string> { nameof(Model.Id) } };
            var json = JsonSerializer.Serialize(query);
            var model = new Model { Id = 1 };
            var models = new List<Model> { model };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _preserver.Setup(m => m.QueryReadAsync<Model>(It.IsAny<Query>())).ReturnsAsync(models);

            var result = await _controller.QueryReadAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);

            var typedResult = JsonSerializer.Deserialize(result.Value!.ToString()!, typeof(IList<object>)) as IList<object>;

            Assert.NotNull(typedResult);
            Assert.Single(typedResult);

            var firstResult = typedResult[0].ToString();

            Assert.Contains(nameof(Model.Id).Camelize(), firstResult);
            Assert.DoesNotContain(nameof(Model.Name).Camelize(), firstResult);
            Assert.DoesNotContain(nameof(Model.Description).Camelize(), firstResult);
        }

        [Fact]
        public async Task QueryReadAsync_QueryHasExcludes_ReturnObjectWithOnlyPropertiesNotRequestedInExcludes()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Excludes = new HashSet<string> { nameof(Model.Name), nameof(Model.Description) } };
            var json = JsonSerializer.Serialize(query);
            var model = new Model { Id = 1 };
            var models = new List<Model> { model };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _preserver.Setup(m => m.QueryReadAsync<Model>(It.IsAny<Query>())).ReturnsAsync(models);

            var result = await _controller.QueryReadAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);

            var typedResult = JsonSerializer.Deserialize(result.Value!.ToString()!, typeof(IList<object>)) as IList<object>;

            Assert.NotNull(typedResult);
            Assert.Single(typedResult);

            var firstResult = typedResult[0].ToString();

            Assert.Contains(nameof(Model.Id).Camelize(), firstResult);
            Assert.DoesNotContain(nameof(Model.Name).Camelize(), firstResult);
            Assert.DoesNotContain(nameof(Model.Description).Camelize(), firstResult);
        }

        [Fact]
        public async Task QueryReadAsync_QueryHasNoIncludesOrExcludes_ReturnFoundModels()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Includes = null, Excludes = null };
            var json = JsonSerializer.Serialize(query);
            var model = new Model { Id = 1 };
            var models = new List<Model> { model };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _preserver.Setup(m => m.QueryReadAsync<Model>(It.IsAny<Query>())).ReturnsAsync(models);

            var result = await _controller.QueryReadAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);

            var typedResult = result.Value as IList<Model>;

            Assert.NotNull(typedResult);
            Assert.Single(typedResult);

            var firstModel = typedResult[0];

            Assert.Equal(model.Id, firstModel.Id);
        }

        [Fact]
        public async Task QueryReadAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.QueryReadAsync(typeName) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        #endregion

        #region QueryReadCountAsync

        [Fact]
        public async Task QueryReadCountAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.QueryReadCountAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task QueryReadCountAsync_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadCountAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task QueryReadCountAsync_QueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = null;
            var json = JsonSerializer.Serialize(query);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadCountAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.BadRequestQuery, $"{nameof(Query)} is null."), result.Value);
        }

        [Fact]
        public async Task QueryReadCountAsync_DeserializeThrowsExceptionQueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var json = @"{ ""OrderBy"": ""1""}";

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryReadCountAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Contains(String.Format(ErrorMessage.BadRequestQuery, String.Empty), result.Value.ToString());
            Assert.Contains("OrderBy", result.Value.ToString());
        }

        [Fact]
        public async Task QueryReadCountAsync_ValidateQueryIsTrueQueryIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = true;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryReadCountAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.", result.Value);
        }

        [Fact]
        public async Task QueryReadCountAsync_ValidateQueryIsFalseQueryIsInvalid_ValidateQueryNotCalled()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryReadCountAsync(typeName);

            _validator.Verify(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>()), Times.Never);
            _preserver.Verify(m => m.QueryReadCountAsync(type, It.Is<Query>(thisQuery => thisQuery.Limit == query.Limit)), Times.Once);
        }

        [Fact]
        public async Task QueryReadCountAsync_CountRead_ReturnsCount()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Includes = null, Excludes = null };
            var json = JsonSerializer.Serialize(query);
            var model = new Model { Id = 1 };
            var models = new List<Model> { model };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _preserver.Setup(m => m.QueryReadCountAsync(It.IsAny<Type>(), It.IsAny<Query>())).ReturnsAsync(models.Count);

            var result = await _controller.QueryReadCountAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);

            var typedResult = (long)result.Value;

            Assert.Equal(models.Count, typedResult);
        }

        [Fact]
        public async Task QueryReadCountAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.QueryReadCountAsync(typeName) as StatusCodeResult;

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
            Model? updatedModel = null;

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
            Model? updatedModel = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Guid>(), It.IsAny<Model>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.PartialUpdateAsync<Model>(It.IsAny<Guid>(), It.IsAny<IDictionary<string, JsonElement>>())).ReturnsAsync(updatedModel);

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
            _preserver.Setup(m => m.PartialUpdateAsync<Model>(It.IsAny<Guid>(), It.IsAny<IDictionary<string, JsonElement>>())).ReturnsAsync(updatedModel);

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

        #region PartialUpdateAsync_WithString
        [Fact]
        public async Task PartialUpdateAsync_WithString_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.PartialUpdateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PartialUpdateAsync_WithString_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.PartialUpdateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithString_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);

            var result = await _controller.PartialUpdateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithString_UpdatedCountReturned_ReturnsUpdatedCount()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            var updatedCount = 1;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidatePartialUpdateAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>?>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.PartialUpdateAsync<Model>(It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, JsonElement>>())).ReturnsAsync(updatedCount);

            var result = await _controller.PartialUpdateAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.True(result.Value is long);
            Assert.Equal(updatedCount, (long)result.Value);
        }

        [Fact]
        public async Task PartialUpdateAsync_WithString_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.PartialUpdateAsync(typeName) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region DeleteAsync_WithStringGuid
        [Fact]
        public async Task DeleteAsync_WithStringGuid_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;
            Guid id = Guid.Empty;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.DeleteAsync(typeName, id) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithStringGuid_DeletedCountIsZero_ReturnsNotFound()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var deletedCount = 0;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.DeleteAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(deletedCount);

            var result = await _controller.DeleteAsync(typeName, id) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.NotFoundDelete, typeName), result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithStringGuid_DeletedCountIsNotZero_ReturnsDeletedCount()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Guid id = Guid.Empty;
            var deletedCount = 1;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _preserver.Setup(m => m.DeleteAsync<Model>(It.IsAny<Guid>())).ReturnsAsync(deletedCount);

            var result = await _controller.DeleteAsync(typeName, id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.True(result.Value is long);
            Assert.Equal(deletedCount, (long)result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithStringGuid_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Guid id = Guid.Empty;
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.DeleteAsync(typeName, id) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region DeleteAsync_WithString
        [Fact]
        public async Task DeleteAsync_WithString_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.DeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithString_ValidationResultIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Message = "some-message"
            };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateDeleteAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>?>())).ReturnsAsync(validationResult);

            var result = await _controller.DeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(validationResult.Message, result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithString_DeletedCountReturned_ReturnsDeletedCount()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var model = new Model { Id = 1 };
            var json = JsonSerializer.Serialize(model);
            var validationResult = new ValidationResult { IsValid = true };
            var deletedCount = 1;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _validator.Setup(m => m.ValidateDeleteAsync(It.IsAny<Model>(), It.IsAny<IDictionary<string, string>?>())).ReturnsAsync(validationResult);
            _preserver.Setup(m => m.DeleteAsync<Model>(It.IsAny<IDictionary<string, string>>())).ReturnsAsync(deletedCount);

            var result = await _controller.DeleteAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.True(result.Value is long);
            Assert.Equal(deletedCount, (long)result.Value);
        }

        [Fact]
        public async Task DeleteAsync_WithString_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            Guid id = Guid.Empty;
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.DeleteAsync(typeName, id) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }
        #endregion

        #region QueryDeleteAsync

        [Fact]
        public async Task QueryDeleteAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);

            var result = await _controller.QueryDeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task QueryDeleteAsync_JsonIsNullOrEmpty_ReturnsBadRequest(String json)
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryDeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }

        [Fact]
        public async Task QueryDeleteAsync_QueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = null;
            var json = JsonSerializer.Serialize(query);

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryDeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(String.Format(ErrorMessage.BadRequestQuery, $"{nameof(Query)} is null."), result.Value);
        }

        [Fact]
        public async Task QueryDeleteAsync_DeserializeThrowsExceptionQueryIsNull_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            var json = @"{ ""OrderBy"": ""1""}";

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);

            var result = await _controller.QueryDeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Contains(String.Format(ErrorMessage.BadRequestQuery, String.Empty), result.Value.ToString());
            Assert.Contains("OrderBy", result.Value.ToString());
        }

        [Fact]
        public async Task QueryDeleteAsync_ValidateQueryIsTrueQueryIsInvalid_ReturnsBadRequest()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = true;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryDeleteAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal($"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.", result.Value);
        }

        [Fact]
        public async Task QueryDeleteAsync_ValidateQueryIsFalseQueryIsInvalid_ValidateQueryNotCalled()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Limit = -1 };
            var json = JsonSerializer.Serialize(query);
            var validationResult = new ValidationResult(false, $"{nameof(Query)} {nameof(Query.Limit)} cannot be less than zero.");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _validator.Setup(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>())).Returns(validationResult);

            var result = await _controller.QueryDeleteAsync(typeName);

            _validator.Verify(m => m.ValidateQuery(It.IsAny<Model>(), It.IsAny<Query>()), Times.Never);
            _preserver.Verify(m => m.QueryDeleteAsync(type, It.Is<Query>(thisQuery => thisQuery.Limit == query.Limit)), Times.Once);
        }

        [Fact]
        public async Task QueryDeleteAsync_ModelsAreDeleted_ReturnsDeletedCount()
        {
            var typeName = "some-type-name";
            Type? type = typeof(Model);
            Query? query = new Query { Includes = null, Excludes = null };
            var json = JsonSerializer.Serialize(query);
            var model = new Model { Id = 1 };
            var models = new List<Model> { model };

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Returns(type);
            _streamService.Setup(m => m.ReadToEndThenDisposeAsync(It.IsAny<Stream>(), It.IsAny<Encoding>())).ReturnsAsync(json);
            _applicationOptions.Value.ValidateQuery = false;
            _preserver.Setup(m => m.QueryDeleteAsync(It.IsAny<Type>(), It.IsAny<Query>())).ReturnsAsync(models.Count);

            var result = await _controller.QueryDeleteAsync(typeName) as OkObjectResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Value);

            var typedResult = (long)result.Value;

            Assert.Equal(models.Count, typedResult);
        }

        [Fact]
        public async Task QueryDeleteAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var typeName = "some-type-name";
            var exception = new Exception("an-error-occurred");

            _typeService.Setup(m => m.GetModelType(It.IsAny<string>())).Throws(exception);

            var result = await _controller.QueryDeleteAsync(typeName) as StatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        }

        #endregion
    }
}
