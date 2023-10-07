namespace Crud.Api.Services.Models
{
    public class MessageResult
    {
        public MessageResult() { }

        public MessageResult(Boolean isSuccessful)
        {
            IsSuccessful = isSuccessful;
        }

        public MessageResult(Boolean isSuccessful, String? message)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }

        public Boolean IsSuccessful { get; set; }
        public String? Message { get; set; }
    }
}
