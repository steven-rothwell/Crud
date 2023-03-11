using System.Text.Json;
using Crud.Api.Models;
using Crud.Api.Options;
using Crud.Api.QueryModels;
using Crud.Api.Services;
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
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetIdFilter(tType, id);

            var bsonDocument = await collection.Find(filter).FirstOrDefaultAsync();
            return bsonDocument.FromBsonDocument<T>();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetQueryParamFilter(tType, queryParams);

            var models = await collection.FindAsync<T>(filter);
            return await models.ToListAsync();
        }

        public async Task<IEnumerable<T>> QueryReadAsync<T>(Query query)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetConditionFilter(tType, query.Where);
            var sort = _mongoDbService.GetSort(query.OrderBy);
            var projections = _mongoDbService.GetProjections(query);

            var models = await collection.FindAsync<T>(filter, new FindOptions<BsonDocument, T>
            {
                Sort = sort,
                Limit = query.Limit,
                Skip = query.Skip,
                Projection = projections
            });
            return await models.ToListAsync();
        }

        public async Task<Int64> QueryReadCountAsync(Type type, Query query)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            string tableName = _mongoDbService.GetTableName(type);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetConditionFilter(type, query.Where);
            var sort = _mongoDbService.GetSort(query.OrderBy);
            var projections = _mongoDbService.GetProjections(query);

            return await collection.CountDocumentsAsync(filter, new CountOptions
            {
                Limit = query.Limit,
                Skip = query.Skip
            });
        }

        public async Task<T> UpdateAsync<T>(Guid id, T model)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = _mongoDbService.GetIdFilter(tType, id);

            var bsonDocument = model.ToBsonDocument();
            return await collection.FindOneAndReplaceAsync<T>(filter, bsonDocument, new FindOneAndReplaceOptions<BsonDocument, T>
            {
                ReturnDocument = ReturnDocument.After
            });
        }

        public async Task<T> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonElement> propertyValues)
        {
            if (propertyValues is null)
                throw new ArgumentNullException(nameof(propertyValues));

            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = _mongoDbService.GetIdFilter(tType, id);
            var updates = _mongoDbService.GetShallowUpdates(propertyValues, tType);  // Can utilize GetDeepUpdates instead, if all child objects are guaranteed to be instantiated.
            var update = Builders<BsonDocument>.Update.Combine(updates);

            return await collection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<BsonDocument, T>
            {
                ReturnDocument = ReturnDocument.After
            });
        }

        public async Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues)
        {
            if (propertyValues is null)
                throw new ArgumentNullException(nameof(propertyValues));

            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);
            var filter = _mongoDbService.GetQueryParamFilter(tType, queryParams);
            var updates = _mongoDbService.GetShallowUpdates(propertyValues, tType);  // Can utilize GetDeepUpdates instead, if all child objects are guaranteed to be instantiated.
            var update = Builders<BsonDocument>.Update.Combine(updates);

            var updateResult = await collection.UpdateManyAsync(filter, update);
            return updateResult.ModifiedCount;
        }

        public async Task<Int64> DeleteAsync<T>(Guid id)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetIdFilter(tType, id);

            var deleteResult = await collection.DeleteOneAsync(filter);
            return deleteResult.DeletedCount;
        }

        public async Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            var tType = typeof(T);
            string tableName = _mongoDbService.GetTableName(tType);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetQueryParamFilter(tType, queryParams);

            var deleteResult = await collection.DeleteManyAsync(filter);
            return deleteResult.DeletedCount;
        }

        public async Task<Int64> QueryDeleteAsync(Type type, Query query)
        {
            var dbClient = new MongoClient(_mongoDbOptions.ConnectionString);
            var database = dbClient.GetDatabase(_mongoDbOptions.DatabaseName);

            string tableName = _mongoDbService.GetTableName(type);
            var collection = database.GetCollection<BsonDocument>(tableName);
            var filter = _mongoDbService.GetConditionFilter(type, query.Where);

            var deleteResult = await collection.DeleteManyAsync(filter);
            return deleteResult.DeletedCount;
        }
    }
}
