using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Crud.Api.Constants;
using Crud.Api.Helpers;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.QueryModels;
using Crud.Api.Services;
using Crud.Api.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Crud.Api.Controllers;

[ApiController]
[Route("api")]
public class CrudController : BaseApiController
{
    private readonly ILogger<CrudController> _logger;
    private readonly IValidator _validator;
    private readonly IPreserver _preserver;
    private readonly IStreamService _streamService;
    private readonly ITypeService _typeService;
    private readonly IQueryCollectionService _queryCollectionService;

    public CrudController(IOptions<ApplicationOptions> applicationOptions, ILogger<CrudController> logger, IValidator validator, IPreserver preserver, IStreamService streamService, ITypeService typeService, IQueryCollectionService queryCollectionService)
        : base(applicationOptions)
    {
        _logger = logger;
        _validator = validator;
        _preserver = preserver;
        _streamService = streamService;
        _typeService = typeService;
        _queryCollectionService = queryCollectionService;
    }

    [Route("{typeName}"), HttpPost]
    public async Task<IActionResult> CreateAsync(String typeName)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidateCreateAsync(model);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var createdModel = await _preserver.CreateAsync(model);

            return Ok(createdModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating with typeName: {typeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName, Guid id)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(Guid) });
            var model = await (dynamic)readAsync.Invoke(_preserver, new object[] { id });

            if (model is null)
                return NotFound(String.Format(ErrorMessage.NotFoundRead, typeName));

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading with typeName: {typeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type);

            var validationResult = (ValidationResult)await _validator.ValidateReadAsync(model!, queryParams);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(IDictionary<String, String>) });
            var models = await (dynamic)readAsync.Invoke(_preserver, new object[] { queryParams });

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading with typeName: {typeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("query/{typeName}"), HttpPost]
    public async Task<IActionResult> QueryReadAsync(String typeName)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            Query? query = null;
            string jsonExMessage = "";
            try { query = JsonSerializer.Deserialize(json, typeof(Query), JsonSerializerOption.Default) as Query; }
            catch (Exception jsonEx)
            {
                jsonExMessage = jsonEx.Message;
            }
            if (query is null)
                return BadRequest(String.Format(ErrorMessage.BadRequestQuery, jsonExMessage));

            // dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type);

            // var validationResult = (ValidationResult)await _validator.ValidateReadAsync(model!, queryParams);
            // if (!validationResult.IsValid)
            //     return BadRequest(validationResult.Message);

            var queryReadAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.QueryReadAsync), new Type[] { typeof(Query) });
            var models = await (dynamic)queryReadAsync.Invoke(_preserver, new object[] { query });

            if ((query.Includes is not null && query.Includes.Count > 0) || (query.Excludes is not null && query.Excludes.Count > 0))
            {
                var modelsWithLessProperties = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(models, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault }));

                return Ok(modelsWithLessProperties);
            }

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading with typeName: {typeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpPut]
    public async Task<IActionResult> UpdateAsync(String typeName, Guid id)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidateUpdateAsync(id, model);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var updatedModel = await _preserver.UpdateAsync(id, model);

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, typeName));

            return Ok(updatedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating with typeName: {typeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName, Guid id)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
            var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(json, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidatePartialUpdateAsync(id, model, propertyValues?.Keys);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var partialUpdateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(Guid), typeof(IDictionary<String, JsonNode>) });
            var updatedModel = await (dynamic)partialUpdateAsync.Invoke(_preserver, new object[] { id, propertyValues });

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, typeName));

            return Ok(updatedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error partially updating with typeName: {typeName}, id {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
            var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(json, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidatePartialUpdateAsync(model, queryParams, propertyValues?.Keys);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var partialUpdateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(IDictionary<String, String>), typeof(IDictionary<String, JsonNode>) });
            var updatedCount = await (dynamic)partialUpdateAsync.Invoke(_preserver, new object[] { queryParams, propertyValues });

            return Ok(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error partially updating with typeName: {typeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName, Guid id)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var deleteAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(Guid) });
            var deletedCount = await (dynamic)deleteAsync.Invoke(_preserver, new object[] { id });

            if (deletedCount == 0)
                return NotFound(String.Format(ErrorMessage.NotFoundDelete, typeName));

            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting with typeName: {typeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName)
    {
        try
        {
            var type = _typeService.GetModelType(typeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type);

            var validationResult = (ValidationResult)await _validator.ValidateDeleteAsync(model!, queryParams);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var deleteAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(IDictionary<String, String>) });
            var deletedCount = await (dynamic)deleteAsync.Invoke(_preserver, new object[] { queryParams });

            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting with typeName: {typeName}.");
            return InternalServerError(ex);
        }
    }
}
