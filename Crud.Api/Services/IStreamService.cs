using System.Text;

namespace Crud.Api.Services
{
    public interface IStreamService
    {
        Task<String> ReadToEndThenDisposeAsync(Stream stream, Encoding encoding);
    }
}
