using System.Net;

namespace Domain.Dtos
{
    public class SapResponse
    {
        public HttpStatusCode Code { get; set; }
        public string Response { get; set; }
    }
}
