using System.Text.Json;
using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.Services;
using Crud.Api.Validators;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IValidator, Validator>();
builder.Services.AddScoped<IPreserver, Crud.Api.Preservers.MongoDb.Preserver>();
builder.Services.AddScoped<IMongoDbService, MongoDbService>();
builder.Services.AddScoped<IQueryCollectionService, QueryCollectionService>();
builder.Services.AddScoped<IStreamService, StreamService>();
builder.Services.AddScoped<ITypeService, TypeService>();

var conventionPack = new ConventionPack
{
    new CamelCaseElementNameConvention(),
    new IgnoreExtraElementsConvention(true)
};
ConventionRegistry.Register("conventionPack", conventionPack, t => true);

// This is necessary for IEnumerable<dynamic> to properly choose the correct GuidRepresentation when dynamic is Guid.
// This is currently a bug in the MongoDB C# driver. This may be removed when fixed.
// https://jira.mongodb.org/browse/CSHARP-4784
var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(typeof(object));
var objectSerializer = new ObjectSerializer(discriminatorConvention, GuidRepresentation.Standard);
BsonSerializer.RegisterSerializer(objectSerializer);

BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(nameof(MongoDbOptions)));
builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection(nameof(ApplicationOptions)));

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
