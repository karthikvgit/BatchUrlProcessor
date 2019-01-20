using Newtonsoft.Json;
using System.Collections.Generic;

namespace BatchUrlProcessor.Models
{
    public class Response
    {
        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public List<UrlResponse> urlResponses { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }
    }
}
