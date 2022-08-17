using System;
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

            return model;
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

            var bsonDocuments = await collection.Find(filter).ToListAsync();

            return bsonDocuments.Select(bsonDocument => bsonDocument.FromBsonDocument<T>());
        }

        public async Task<T> UpdateAsync<T>(T model, Guid id)
        {
            if (model is null)
                throw new Exception("Cannot update because model is null.");

            var dbClient = new MongoClient("mongodb://localhost");

            var database = dbClient.GetDatabase("testDb");

            string? tableName = model.GetTableName();
            if (tableName is null)
                throw new Exception($"No table name found on {model.GetType().Name}.");

            var collection = database.GetCollection<BsonDocument>(tableName, _mongoCollectionSettings);

            FilterDefinition<BsonDocument> filter;
            if (model is IExternalEntity)
                filter = Builders<BsonDocument>.Filter.Eq(nameof(IExternalEntity.ExternalId), id);
            else
                filter = Builders<BsonDocument>.Filter.Eq("Id", id);

            var bsonDocument = model.ToBsonDocument();


            await collection.InsertOneAsync(bsonDocument);

            return model;
        }
    }
}
