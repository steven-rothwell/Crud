using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Crud.Api.Constants;
using Crud.Api.Models;
using Humanizer;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crud.Api.Preservers.MongoDb
{
    public class Preserver : IPreserver
    {
        private readonly String _connectionString;
        private readonly MongoCollectionSettings _mongoCollectionSettings;

        public Preserver()//IOptions<SqlServerOptions> options)
        {
            //_connectionString = options.Value.ConnectionString;
            _mongoCollectionSettings = new MongoCollectionSettings
            {
                AssignIdOnInsert = true
            };
        }

        public async Task<T> CreateAsync<T>(T model)
        {
            if (model is null)
                throw new Exception("Cannot create because model is null.");

            if (model is IExternalEntity entity && !entity.ExternalId.HasValue)
            {
                entity.ExternalId = Guid.NewGuid();
            }

            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            string? tableName = model.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {model.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            var bsonDocument = model.ToBsonDocument();

            await collection.InsertOneAsync(bsonDocument);

            return bsonDocument.FromBsonDocument<T>();
        }

        public async Task<T> ReadAsync<T>(Guid id)
        {
            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            var tType = typeof(T);
            string? tableName = tType.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {tType.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName);

            FilterDefinition<BsonDocument> filter;
            if (typeof(IExternalEntity).IsAssignableFrom(tType))
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

            var bsonDocument = await collection.Find(filter).FirstOrDefaultAsync();

            return bsonDocument.FromBsonDocument<T>();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams)
        {
            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            var tType = typeof(T);
            string? tableName = tType.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {tType.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName);

            FilterDefinition<BsonDocument> filter = new BsonDocument();
            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    string key = queryParam.Key.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic value = queryParam.Value.ChangeType(tType.GetProperties().GetProperty(key, Delimiter.MongoDbChildProperty)!.PropertyType);
                    filter &= Builders<BsonDocument>.Filter.Eq(key.Pascalize(Delimiter.MongoDbChildProperty), value);
                }
            }

            var models = await collection.FindAsync<T>(filter);

            return await models.ToListAsync();
        }

        public async Task<T> UpdateAsync<T>(Guid id, T model)
        {
            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            var tType = typeof(T);
            string? tableName = tType.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {tType.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            FilterDefinition<BsonDocument> filter;
            if (typeof(IExternalEntity).IsAssignableFrom(tType))
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

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

            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            var tType = typeof(T);
            string? tableName = tType.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {tType.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            FilterDefinition<BsonDocument> filter;
            if (typeof(IExternalEntity).IsAssignableFrom(tType))
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

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

            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            var tType = typeof(T);
            string? tableName = tType.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {tType.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            FilterDefinition<BsonDocument> filter = new BsonDocument();
            if (queryParams is not null)
            {
                foreach (var queryParam in queryParams)
                {
                    string key = queryParam.Key.Replace(Delimiter.QueryParamChildProperty, Delimiter.MongoDbChildProperty);
                    dynamic value = queryParam.Value.ChangeType(tType.GetProperties().GetProperty(key, Delimiter.MongoDbChildProperty)!.PropertyType);
                    filter &= Builders<BsonDocument>.Filter.Eq(key.Pascalize(Delimiter.MongoDbChildProperty), value);
                }
            }

            var updates = GetShallowUpdates(propertyValues, tType);  // Can utilize GetDeepUpdates instead, if all child objects are guaranteed to be instantiated.

            var update = Builders<BsonDocument>.Update.Combine(updates);

            var updateResult = await collection.UpdateManyAsync(filter, update);

            return updateResult.ModifiedCount;
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
            string currentPropertyName = propertyName.ValueAfterLastDelimiter(Delimiter.MongoDbChildProperty);

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
