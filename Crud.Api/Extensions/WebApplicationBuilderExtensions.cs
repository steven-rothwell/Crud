using Crud.Api.Options;
using Crud.Api.Preservers;
using Crud.Api.Preservers.EntityFramework;
using Crud.Api.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Crud.Api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void UseMongoDb(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPreserver, Preservers.MongoDb.Preserver>();
            builder.Services.AddScoped<IMongoDbService, MongoDbService>();

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
        }

        public static void UseSqlServer(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPreserver, Preservers.EntityFramework.Preserver>();
            builder.Services.AddScoped<IEntityFrameworkService, EntityFrameworkService>();

            builder.Services.AddDbContext<EFDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
        }
    }
}
