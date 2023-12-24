using System.Text.Json;
using Crud.Api.Extensions;
using Crud.Api.Options;
using Crud.Api.Services;
using Crud.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IValidator, Validator>();
builder.Services.AddScoped<IQueryCollectionService, QueryCollectionService>();
builder.Services.AddScoped<IStreamService, StreamService>();
builder.Services.AddScoped<ITypeService, TypeService>();
builder.Services.AddScoped<IPreprocessingService, PreprocessingService>();
builder.Services.AddScoped<IPostprocessingService, PostprocessingService>();
builder.Services.AddScoped<ISanitizerService, SanitizerService>();

builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection(nameof(ApplicationOptions)));

//builder.UseMongoDb();
builder.UseSqlServer();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();