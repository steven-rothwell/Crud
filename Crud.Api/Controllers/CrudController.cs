using System.Reflection;
using System.Text;
using Crud.Api.Constants;
using Crud.Api.Helpers;
using Crud.Api.Preservers;
using Crud.Api.Validators;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Crud.Api.Controllers;

[ApiController]
[Route("api")]
public class CrudController : BaseApiController<CrudController>
{
    private readonly IValidator _validator;
    private readonly IPreserver _preserver;

    public CrudController(ILogger<CrudController> logger, IValidator validator, IPreserver preserver)
        : base(logger)
    {
        _validator = validator;
        _preserver = preserver;
    }

    [Route("{typeName}"), HttpPost]
    public async Task<IActionResult> CreateAsync(String typeName)
    {
        // TODO: TRY/Catch error handling
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            string json = await reader.ReadToEndAsync();
            var type = Type.GetType($"{Namespace.Models}.{typeName.Singularize().Pascalize()}");

            if (String.IsNullOrWhiteSpace(json) || type is null)
                return Ok();  // TODO: return error.

            dynamic? model = JsonConvert.DeserializeObject(json, type);

            var isValid = await _validator.ValidateCreateAsync(model);

            if (isValid)
            {
                await _preserver.CreateAsync(model);
            }

            return Ok(isValid);
        }
    }

    [Route("{typeName}/{id:guid}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName, Guid id)
    {
        var type = Type.GetType($"{Namespace.Models}.{typeName.Singularize().Pascalize()}");

        if (type is null)
            return Ok();  // TODO: return error.

        var readAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.ReadAsync), new Type[] { typeof(Guid) });
        var model = await (dynamic)readAsync.Invoke(_preserver, new object[] { id });

        return Ok(model);
    }

    [Route("{typeName}"), HttpGet]
    public async Task<IActionResult> ReadAsync(String typeName)
    {
        var queryCollection = Request.Query;

        var queryParams = queryCollection.ToDictionary(query => query.Key, query => query.Value.ToString());

        var type = Type.GetType($"{Namespace.Models}.{typeName.Singularize().Pascalize()}");

        if (type is null)
            return Ok();  // TODO: return error.

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
        // TODO: TRY/Catch error handling
        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            string json = await reader.ReadToEndAsync();
            var type = Type.GetType($"{Namespace.Models}.{typeName.Singularize().Pascalize()}");

            if (String.IsNullOrWhiteSpace(json) || type is null)
                return Ok();  // TODO: return error.

            dynamic? model = JsonConvert.DeserializeObject(json, type);

            var isValid = await _validator.ValidateUpdateAsync(id, model);

            if (isValid)
            {
                var propertyValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var updateAsync = ReflectionHelper.GetGenericMethod(type, typeof(IPreserver), nameof(IPreserver.UpdateAsync), new Type[] { typeof(Guid), typeof(IDictionary<String, String>) });
                var updatedModel = await (dynamic)updateAsync.Invoke(_preserver, new object[] { id, propertyValues });

                return Ok(updatedModel);
            }

            // TODO: Do somethign when not valid.
            return Ok("Not Valid");
        }
    }
}
