using Crud.Api.Constants;
using Crud.Api.Controllers;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.Services;
using Crud.Api.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Crud.Api.Tests.Controllers
{
    public class CrudControllerTests
    {
        private IOptions<ApplicationOptions> _applicationOptions;
        private Mock<ILogger<CrudController>> _logger;
        private Mock<IValidator> _validator;
        private Mock<IPreserver> _preserver;
        private Mock<IStreamService> _streamService;
        private Mock<ITypeService> _typeService;
        private CrudController _controller;

        public CrudControllerTests()
        {
            _applicationOptions = Microsoft.Extensions.Options.Options.Create(new ApplicationOptions());
            _logger = new Mock<ILogger<CrudController>>();
            _validator = new Mock<IValidator>();
            _preserver = new Mock<IPreserver>();
            _streamService = new Mock<IStreamService>();
            _typeService = new Mock<ITypeService>();
            _controller = new CrudController(_applicationOptions, _logger.Object, _validator.Object, _preserver.Object, _streamService.Object, _typeService.Object);
        }

        [Fact]
        public async Task CreateAsync_TypeIsNull_ReturnsBadRequest()
        {
            var typeName = "someTypeName";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<String>())).Returns(type);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestModelType, result.Value);
        }

        [Fact]
        public async Task CreateAsync_JsonIsNull_ReturnsBadRequest()
        {
            var typeName = "someTypeName";
            Type? type = null;

            _typeService.Setup(m => m.GetModelType(It.IsAny<String>())).Returns(type);

            var result = await _controller.CreateAsync(typeName) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(ErrorMessage.BadRequestBody, result.Value);
        }
    }
}
