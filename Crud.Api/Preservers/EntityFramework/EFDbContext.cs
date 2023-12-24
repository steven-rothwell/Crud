using System.Reflection;
using Crud.Api.Constants;
using Microsoft.EntityFrameworkCore;

namespace Crud.Api.Preservers.EntityFramework
{
    public class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var modelTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == Namespace.Models && t.IsClass && !t.IsAbstract);

            foreach (var modelType in modelTypes)
            {
                // User and Address are used for no-sql. This logic may be removed in forked applications.
                if (modelType == typeof(Crud.Api.Models.User) || modelType == typeof(Crud.Api.Models.Address))
                    continue;

                var tableName = modelType.GetTableName();
                modelBuilder.Entity(modelType).ToTable(tableName);
            }
        }
    }
}
