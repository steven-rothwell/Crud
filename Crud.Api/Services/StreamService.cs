using System.Text;

namespace Crud.Api.Services
{
    public class StreamService : IStreamService
    {
        public StreamService() { }

        public async Task<String> ReadToEndThenDisposeAsync(Stream stream, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(stream, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
