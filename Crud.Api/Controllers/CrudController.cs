using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Functions;
using Crud.Api.Helpers;
using Crud.Api.Preservers;
using Crud.Api.Services;
using Crud.Api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace Crud.Api.Controllers;

[ApiController]
[Route("api")]
public class CrudController : BaseApiController<CrudController>
{
    private readonly IValidator _validator;
    private readonly IPreserver _preserver;
    private readonly IStreamService _streamService;

    public CrudController(ILogger<CrudController> logger, IValidator validator, IPreserver preserver, IStreamService streamService)
        : base(logger)
    {
        _validator = validator;
        _preserver = preserver;
        _streamService = streamService;
    }

    [Route("{typeName}"), HttpPost]
    public async Task<IActionResult> CreateAsync(String typeName)
    {
        string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
        if (String.IsNullOrWhiteSpace(json))
            return BadRequest(ErrorMessage.BadRequestBody);

        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

        var isValid = await _validator.ValidateCreateAsync(model);

        if (isValid)
        {
            var createdModel = await _preserver.CreateAsync(model);

            return Ok(createdModel);
        }

        return Ok(isValid);
    }

    [Route("{typeName}/{id:guid}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName, Guid id)
    {
        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(Guid) });
        var model = await (dynamic)readAsync.Invoke(_preserver, new object[] { id });

        if (model is null)
            return NotFound(String.Format(ErrorMessage.NotFoundRead, typeName));

        return Ok(model);
    }

    [Route("{typeName}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName)
    {
        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        var queryCollection = Request.Query;

        var queryParams = queryCollection.ToDictionary(query => query.Key, query => query.Value.ToString());

        dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type);

        var isValid = await _validator.ValidateReadAsync(model!, queryParams);

        if (isValid)
        {
            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(IDictionary<String, String>) });
            var models = await (dynamic)readAsync.Invoke(_preserver, new object[] { queryParams });

            return Ok(models);
        }

        // TODO: Do somethign when not valid.
        return Ok("Not Valid");
    }

    [Route("{typeName}/{id:guid}"), HttpPut]
    public async Task<IActionResult> UpdateAsync(String typeName, Guid id)
    {
        string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
        if (String.IsNullOrWhiteSpace(json))
            return BadRequest(ErrorMessage.BadRequestBody);

        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);

        var isValid = await _validator.ValidateUpdateAsync(id, model);

        if (isValid)
        {
            var updatedModel = await _preserver.UpdateAsync(id, model);

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, typeName));

            return Ok(updatedModel);
        }

        // TODO: Do somethign when not valid.
        return Ok("Not Valid");
    }

    [Route("{typeName}/{id:guid}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName, Guid id)
    {
        string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
        if (String.IsNullOrWhiteSpace(json))
            return BadRequest(ErrorMessage.BadRequestBody);

        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
        var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(json, JsonSerializerOption.Default);

        var isValid = await _validator.ValidatePartialUpdateAsync(id, model, propertyValues?.Keys);

        if (isValid)
        {
            var updateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(Guid), typeof(IDictionary<String, JsonNode>) });
            var updatedModel = await (dynamic)updateAsync.Invoke(_preserver, new object[] { id, propertyValues });

            if (updatedModel is null)
                return NotFound(String.Format(ErrorMessage.NotFoundUpdate, typeName));

            return Ok(updatedModel);
        }

        // TODO: Do somethign when not valid.
        return Ok("Not Valid");
    }

    [Route("{typeName}"), HttpPatch]
    public async Task<IActionResult> PartialUpdateAsync(String typeName)
    {
        string json = await _streamService.ReadToEndThenDisposeAsync(Request.Body, Encoding.UTF8);
        if (String.IsNullOrWhiteSpace(json))
            return BadRequest(ErrorMessage.BadRequestBody);

        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        var queryCollection = Request.Query;

        var queryParams = queryCollection.ToDictionary(query => query.Key, query => query.Value.ToString());

        dynamic? model = JsonSerializer.Deserialize(json, type, JsonSerializerOption.Default);
        var propertyValues = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(json, JsonSerializerOption.Default);

        var isValid = await _validator.ValidatePartialUpdateAsync(model, queryParams, propertyValues?.Keys);

        if (isValid)
        {
            var updateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.PartialUpdateAsync), new Type[] { typeof(IDictionary<String, String>), typeof(IDictionary<String, JsonNode>) });
            var updatedCount = await (dynamic)updateAsync.Invoke(_preserver, new object[] { queryParams, propertyValues });

            return Ok(updatedCount);
        }

        // TODO: Do somethign when not valid.
        return Ok("Not Valid");
    }

    [Route("{typeName}/{id:guid}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName, Guid id)
    {
        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(Guid) });
        var deletedCount = await (dynamic)readAsync.Invoke(_preserver, new object[] { id });

        if (deletedCount == 0)
            return NotFound(String.Format(ErrorMessage.NotFoundDelete, typeName));

        return Ok(deletedCount);
    }

    [Route("{typeName}"), HttpDelete]
    public async Task<IActionResult> DeleteAsync(String typeName)
    {
        var type = TypeFunc.GetModelType(typeName);
        if (type is null)
            return BadRequest(ErrorMessage.BadRequestModelType);

        var queryCollection = Request.Query;

        var queryParams = queryCollection.ToDictionary(query => query.Key, query => query.Value.ToString());

        dynamic model = Convert.ChangeType(Activator.CreateInstance(type, null), type);

        var isValid = await _validator.ValidateDeleteAsync(model!, queryParams);

        if (isValid)
        {
            var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.DeleteAsync), new Type[] { typeof(IDictionary<String, String>) });
            var deletedCount = await (dynamic)readAsync.Invoke(_preserver, new object[] { queryParams });

            return Ok(deletedCount);
        }

        // TODO: Do somethign when not valid.
        return Ok("Not Valid");
    }
}
