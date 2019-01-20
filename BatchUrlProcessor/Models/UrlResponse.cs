using Newtonsoft.Json;
using System.Net;

namespace BatchUrlProcessor.Models
{
    public class UrlResponse
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("response_status_code", NullValueHandling = NullValueHandling.Ignore)]
        public HttpStatusCode statusCode { get; set; }

        [JsonProperty("Title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("Protocol", NullValueHandling = NullValueHandling.Ignore)]
        public Protocol Protocol { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }

    }
}
