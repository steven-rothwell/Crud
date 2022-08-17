// using Crud.Api.Models;
// using Crud.Api.Options;
// using Dapper;
// using Microsoft.Data.SqlClient;
// using Microsoft.Extensions.Options;

// namespace Crud.Api.Preservers.Dapper.SqlServer
// {
//     public class Preserver : IPreserver
//     {
//         private readonly String _connectionString;

//         public Preserver(IOptions<SqlServerOptions> options)
//         {
//             _connectionString = options.Value.ConnectionString;
//         }

//         public async Task<T> CreateAsync<T>(T model)
//         {
//             if (model is IExternalEntity entity && !entity.ExternalId.HasValue)
//             {
//                 entity.ExternalId = Guid.NewGuid();
//             }

//             using (var connection = new SqlConnection(_connectionString))
//             {
//                 await connection.InsertAsync<Guid, T>(model);
//             }

//             return model;
//         }

//         public async Task<T> ReadAsync<T>(Guid id)
//         {
//             using (var connection = new SqlConnection(_connectionString))
//             {
//                 return await connection.GetAsync<T>(id);
//             }
//         }
//     }
// }
