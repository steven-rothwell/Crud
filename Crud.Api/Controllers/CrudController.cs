using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crud.Api.Constants;
using Crud.Api.Enums;
using Crud.Api.Helpers;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.QueryModels;
using Crud.Api.Results;
using Crud.Api.Services;
using Crud.Api.Validators;
using Humanizer;
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
    private readonly IPreprocessingService _preprocessingService;
    private readonly IPostprocessingService _postprocessingService;
    private readonly ISanitizerService _sanitizerService;

    public CrudController(IOptions<ApplicationOptions> applicationOptions, ILogger<CrudController> logger, IValidator validator, IPreserver preserver, IStreamService streamService, ITypeService typeService, IQueryCollectionService queryCollectionService,
        IPreprocessingService preprocessingService, IPostprocessingService postprocessingService, ISanitizerService sanitizerService)
        : base(applicationOptions)
    {
        _logger = logger;
        _validator = validator;
        _preserver = preserver;
        _streamService = streamService;
        _typeService = typeService;
        _queryCollectionService = queryCollectionService;
        _preprocessingService = preprocessingService;
        _postprocessingService = postprocessingService;
        _sanitizerService = sanitizerService;
    }

    [Route("{typeName}"), HttpPost]
    public async Task<IActionResult> CreateAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.Create;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidateCreateAsync(model);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessCreateAsync(model);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var createdModel = await _preserver.CreateAsync(model);

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessCreateAsync(createdModel);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(createdModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName, Guid id)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.ReadWithId;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessReadAsync(model, id);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(Guid) });
            model = await (dynamic)readAsync.Invoke(_preserver, new object[] { id });

            if (model is null)
                return NotFound(String.Format(ErrorMessage.NotFoundRead, sanitizedTypeName));

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessReadAsync(model, id);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading with typeName: {sanitizedTypeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.ReadWithQueryParams;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            var validationResult = (ValidationResult)await _validator.ValidateReadAsync(model!, queryParams);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessReadAsync(model!, queryParams);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(IDictionary<String, String>) });
            var models = await (dynamic)readAsync.Invoke(_preserver, new object[] { queryParams });

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessReadAsync(models, queryParams);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("query/{typeName}"), HttpPost]
    public async Task<IActionResult> QueryReadAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.ReadWithQuery;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            Query? query = null;
            string jsonExMessage = $"{nameof(Query)} is null.";
            try { query = JsonSerializer.Deserialize(json, typeof(Query), JsonSerializerOption.Default) as Query; }
            catch (Exception jsonEx)
            {
                jsonExMessage = jsonEx.Message;
            }
            if (query is null)
                return BadRequest(String.Format(ErrorMessage.BadRequestQuery, jsonExMessage));

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            if (_applicationOptions.ValidateQuery)
            {
                var validationResult = (ValidationResult)_validator.ValidateQuery(model!, query);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Message);
            }

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessReadAsync(model!, query);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var queryReadAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.QueryReadAsync), new Type[] { typeof(Query) });
            var models = await (dynamic)queryReadAsync.Invoke(_preserver, new object[] { query });

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessReadAsync(models, query);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            if ((query.Includes is not null && query.Includes.Count > 0) || (query.Excludes is not null && query.Excludes.Count > 0))
            {
                var modelsWithLessProperties = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(models, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

                return Ok(modelsWithLessProperties);
            }

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error query reading with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("query/{typeName}/count"), HttpPost]
    public async Task<IActionResult> QueryReadCountAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.ReadCount;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            Query? query = null;
            string jsonExMessage = $"{nameof(Query)} is null.";
            try { query = JsonSerializer.Deserialize(json, typeof(Query), JsonSerializerOption.Default) as Query; }
            catch (Exception jsonEx)
            {
                jsonExMessage = jsonEx.Message;
            }
            if (query is null)
                return BadRequest(String.Format(ErrorMessage.BadRequestQuery, jsonExMessage));

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            if (_applicationOptions.ValidateQuery)
            {
                var validationResult = (ValidationResult)_validator.ValidateQuery(model!, query);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Message);
            }

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessReadCountAsync(model!, query);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            long count = await _preserver.QueryReadCountAsync(type, query);

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessReadCountAsync(model!, query, count);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error query reading count with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpPut]
    public async Task<IActionResult> UpdateAsync(String typeName, Guid id)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.Update;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidateUpdateAsync(model, id);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessUpdateAsync(model, id);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var updatedModel = await _preserver.UpdateAsync(model, id);

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, sanitizedTypeName));

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessUpdateAsync(updatedModel, id);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(updatedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating with typeName: {sanitizedTypeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName, Guid id)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.PartialUpdateWithId;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
            var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidatePartialUpdateAsync(model, id, propertyValues?.Keys);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessPartialUpdateAsync(model, id, propertyValues);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var partialUpdateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(Guid), typeof(IDictionary<String, JsonElement>) });
            var updatedModel = await (dynamic)partialUpdateAsync.Invoke(_preserver, new object[] { id, propertyValues });

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, sanitizedTypeName));

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessPartialUpdateAsync(updatedModel, id, propertyValues);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(updatedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error partially updating with typeName: {sanitizedTypeName}, id {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.PartialUpdateWithQueryParams;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
            var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonSerializerOption.Default);

            var validationResult = (ValidationResult)await _validator.ValidatePartialUpdateAsync(model, queryParams, propertyValues?.Keys);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessPartialUpdateAsync(model, queryParams, propertyValues);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var partialUpdateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(IDictionary<String, String>), typeof(IDictionary<String, JsonElement>) });
            var updatedCount = await (dynamic)partialUpdateAsync.Invoke(_preserver, new object[] { queryParams, propertyValues });

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessPartialUpdateAsync(model, queryParams, propertyValues, updatedCount);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error partially updating with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName, Guid id)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.DeleteWithId;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessDeleteAsync(model, id);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var deleteAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(Guid) });
            var deletedCount = await (dynamic)deleteAsync.Invoke(_preserver, new object[] { id });

            if (deletedCount == 0)
                return NotFound(String.Format(ErrorMessage.NotFoundDelete, sanitizedTypeName));

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessDeleteAsync(model, id, deletedCount);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting with typeName: {sanitizedTypeName}, id: {id}.");
            return InternalServerError(ex);
        }
    }

    [Route("{typeName}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.DeleteWithQueryParams;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            var queryParams = _queryCollectionService.ConvertToDictionary(Request.Query);

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            var validationResult = (ValidationResult)await _validator.ValidateDeleteAsync(model!, queryParams);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessDeleteAsync(model, queryParams);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var deleteAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(IDictionary<String, String>) });
            var deletedCount = await (dynamic)deleteAsync.Invoke(_preserver, new object[] { queryParams });

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessDeleteAsync(model, queryParams, deletedCount);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }

    [Route("query/{typeName}"), HttpDelete]
    public async Task<IActionResult> QueryDeleteAsync(String typeName)
    {
        var sanitizedTypeName = Default.TypeName;

        try
        {
            sanitizedTypeName = _sanitizerService.SanitizeTypeName(typeName);

            var type = _typeService.GetModelType(sanitizedTypeName);
            if (type is null)
                return BadRequest(ErrorMessage.BadRequestModelType);

            var crudOperation = CrudOperation.DeleteWithQuery;
            if (!type.AllowsCrudOperation(crudOperation))
                return MethodNotAllowed(String.Format(ErrorMessage.MethodNotAllowedType, crudOperation.ToString().Humanize(), type.Name));

            string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
            if (String.IsNullOrWhiteSpace(json))
                return BadRequest(ErrorMessage.BadRequestBody);

            Query? query = null;
            string jsonExMessage = $"{nameof(Query)} is null.";
            try { query = JsonSerializer.Deserialize(json, typeof(Query), JsonSerializerOption.Default) as Query; }
            catch (Exception jsonEx)
            {
                jsonExMessage = jsonEx.Message;
            }
            if (query is null)
                return BadRequest(String.Format(ErrorMessage.BadRequestQuery, jsonExMessage));

            dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type)!;

            if (_applicationOptions.ValidateQuery)
            {
                var validationResult = (ValidationResult)_validator.ValidateQuery(model!, query);
                if (!validationResult.IsValid)
                    return BadRequest(validationResult.Message);
            }

            var preprocessingMessageResult = (MessageResult)await _preprocessingService.PreprocessDeleteAsync(model, query);
            if (!preprocessingMessageResult.IsSuccessful)
                return InternalServerError(preprocessingMessageResult.Message);

            var deletedCount = await _preserver.QueryDeleteAsync(type, query);

            var postprocessingMessageResult = (MessageResult)await _postprocessingService.PostprocessDeleteAsync(model, query, deletedCount);
            if (!postprocessingMessageResult.IsSuccessful)
                return InternalServerError(postprocessingMessageResult.Message);

            return Ok(deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting with typeName: {sanitizedTypeName}.");
            return InternalServerError(ex);
        }
    }
}
