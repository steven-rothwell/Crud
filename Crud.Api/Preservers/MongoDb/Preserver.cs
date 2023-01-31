using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Models;
using Crud.Api.Options;
using Crud.Api.QueryModels;
using Crud.Api.Services;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Preservers.MongoDb
{
    public class Preserver : IPreserver
    {
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly MongoCollectionSettings _mongoCollectionSettings;
        private readonly IMongoDbService _mongoDbService;

        public Preserver(IOptions<MongoDbOptions> mongoDbOptions, IMongoDbService mongoDbService)
        {
            _mongoDbOptions = mongoDbOptions.Value;
            _mongoCollectionSettings = new MongoCollectionSettings
            {
                AssignIdOnInsert = true
            };
            _mongoDbService = mongoDbService;
        }

        public async Task<T> CreateAsync<T>(T model)
        {
            if (model is null)
                throw new Exception("Cannot create because model is null.");

            if (model is IExternalEntity entity && !entity.ExternalId.HasValue)
            {
                entity.ExternalId = Guid.NewGuid();
            }

            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            string? tableName = model.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {model.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            var bsonDocument = model.ToBsonDocument();
            await collection.InsertOneAsync(bsonDocument);
            return bsonDocument.FromBsonDocument<T>();
        }

        public async Task<T?> ReadAsync<T>(Guid id)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = GetIdFilter(tType, id);

            var bsonDocument = await collection.Find(filter).FirstOrDefaultAsync();
            return bsonDocument.FromBsonDocument<T>();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = GetQueryParamFilter(tType, queryParams);

            var models = await collection.FindAsync<T>(filter);
            return await models.ToListAsync();
        }

        public async Task<IEnumerable<T>> QueryReadAsync<T>(Query query)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetConditionsFilter(tType, query.Where);

            var models = await collection.FindAsync<T>(filter);
            return await models.ToListAsync();
        }

        public async Task<T> UpdateAsync<T>(Guid id, T model)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = GetIdFilter(tType, id);

            var bsonDocument = model.ToBsonDocument();
            return await collection.FindOneAndReplaceAsync<T>(filter, bsonDocument, new FindOneAndReplaceOptions<BsonDocument, T>
            {
                ReturnDocument = ReturnDocument.After
            });
        }

        public async Task<T> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonNode> propertyValues)
        {
            if (propertyValues is null)
                throw new ArgumentNullException(nameof(propertyValues));

            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = GetIdFilter(tType, id);
            var updates = GetShallowUpdates(propertyValues, tType);  // Can utilize GetDeepUpdates instead, if all child objects are guaranteed to be instantiated.
            var update = Builders<BsonDocument>.Update.Combine(updates);

            return await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<BsonDocument, T>
            {
                ReturnDocument = ReturnDocument.After
            });
        }

        public async Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonNode> propertyValues)
        {
            if (propertyValues is null)
                throw new ArgumentNullException(nameof(propertyValues));

            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = GetQueryParamFilter(tType, queryParams);
            var updates = GetShallowUpdates(propertyValues, tType);  // Can utilize GetDeepUpdates instead, if all child objects are guaranteed to be instantiated.
            var update = Builders<BsonDocument>.Update.Combine(updates);

            var updateResult = await collection.UpdateManyAsync(filter, update);
            return updateResult.ModifiedCount;
        }

        public async Task<Int64> DeleteAsync<T>(Guid id)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = GetIdFilter(tType, id);

            var deleteResult = await collection.DeleteOneAsync(filter);
            return deleteResult.DeletedCount;
        }

        public async Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = GetQueryParamFilter(tType, queryParams);

            var deleteResult = await collection.DeleteManyAsync(filter);
            return deleteResult.DeletedCount;
        }

        private String GetTableName(Type type)
        {
            string? tableName = type.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {type.GetType().Name}.");

            return tableName;
        }

        private FilterDefinition<BsonDocument> GetIdFilter(Type type, Guid id)
        {
            FilterDefinition<BsonDocument> filter;
            if (typeof(IExternalEntity).IsAssignableFrom(type))
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

            return filter;
        }

        private FilterDefinition<BsonDocument> GetQueryParamFilter(Type type, IDictionary<String, String>? queryParams)
        {
            FilterDefinition<BsonDocument> filter = new BsonDocument();
            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    string key = queryParam.Key.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic value = queryParam.Value.ChangeType(type.GetProperties().GetProperty(key, Delimiter.MongoDbChildProperty)!.PropertyType);
                    filter &= Builders<BsonDocument>.Filter.Eq(key.Pascalize(Delimiter.MongoDbChildProperty), value);
                }
            }

            return filter;
        }

        private IEnumerable<UpdateDefinition<BsonDocument>> GetShallowUpdates(IDictionary<String, JsonNode> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Pascalize();
                dynamic value = JsonSerializer.Deserialize(propertyValue.Value, type.GetProperty(key)!.PropertyType, JsonSerializerOption.Default);
                updates.Add(Builders<BsonDocument>.Update.Set(key, value));
            }

            return updates;
        }

        private IEnumerable<UpdateDefinition<BsonDocument>> GetDeepUpdates(IDictionary<String, JsonNode> propertyValues, Type type)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();

            foreach (var propertyValue in propertyValues)
            {
                string key = propertyValue.Key.Pascalize();
                updates.AddRange(GetAllPropertiesToUpdate(key, type, propertyValue.Value));
            }

            return updates;
        }

        private IEnumerable<UpdateDefinition<BsonDocument>> GetAllPropertiesToUpdate(String propertyName, Type type, JsonNode jsonNode)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            string currentPropertyName = propertyName.GetValueAfterLastDelimiter(Delimiter.MongoDbChildProperty);

            if (jsonNode is JsonObject)
            {
                var propertyValues = jsonNode.Deserialize<Dictionary<string, JsonNode>>();
                foreach (var propertyValue in propertyValues!)
                {
                    updates.AddRange(GetAllPropertiesToUpdate($"{propertyName}{Delimiter.MongoDbChildProperty}{propertyValue.Key.Pascalize()}", type.GetProperty(currentPropertyName)!.PropertyType, propertyValue.Value));
                }
            }
            else
            {
                dynamic? value = jsonNode.Deserialize(type.GetProperty(currentPropertyName)!.PropertyType, JsonSerializerOption.Default);
                updates.Add(Builders<BsonDocument>.Update.Set(propertyName, value));
            }

            return updates;
        }
    }
}
