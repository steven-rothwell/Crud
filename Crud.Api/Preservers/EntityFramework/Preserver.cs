using System.Text.Json;
using Crud.Api.Models;
using Crud.Api.QueryModels;
using Crud.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Crud.Api.Preservers.EntityFramework
{
    public class Preserver : IPreserver
    {
        private readonly EFDbContext _context;
        private readonly IEntityFrameworkService _entityFrameworkService;

        public Preserver(EFDbContext context, IEntityFrameworkService entityFrameworkService)
        {
            _context = context;
            _entityFrameworkService = entityFrameworkService;
        }

        public async Task<T> CreateAsync<T>(T model)
        {
            if (model is null)
                throw new Exception("Cannot create because model is null.");

            if (model is IExternalEntity entity && !entity.ExternalId.HasValue)
            {
                entity.ExternalId = Guid.NewGuid();
            }

            await _context.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public Task<T?> ReadAsync<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(IDictionary<String, String>? queryParams) where T : class
        {
            var expressions = _entityFrameworkService.GetQueryParamFilterExpressions<T>(queryParams);

            IQueryable<T> query = _context.Set<T>();
            foreach (var expression in expressions)
            {
                query = query.Where(expression);
            }

            return await query.ToListAsync();
        }

        public Task<IEnumerable<T>> QueryReadAsync<T>(Query query)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> QueryReadCountAsync(Type type, Query query)
        {
            throw new NotImplementedException();
        }

        public Task<T?> UpdateAsync<T>(T model, Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<T?> PartialUpdateAsync<T>(Guid id, IDictionary<String, JsonElement> propertyValues)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> PartialUpdateAsync<T>(IDictionary<String, String>? queryParams, IDictionary<String, JsonElement> propertyValues)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> DeleteAsync<T>(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> DeleteAsync<T>(IDictionary<String, String>? queryParams)
        {
            throw new NotImplementedException();
        }

        public Task<Int64> QueryDeleteAsync(Type type, Query query)
        {
            throw new NotImplementedException();
        }
    }
}
