using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Common.Utilities.Helpers
{
    public interface IHttpClientHelper
    {
        Task<HttpResponseMessage> GetResponseMessage(object value, Uri fullUrl, string authorizationScheme,
                                                        string authorizationValue, HttpMethod method = null,
                                                        TimeSpan timeout = default(TimeSpan),
                                                        Dictionary<string, IEnumerable<string>> requestHeaders = null,
                                                        Dictionary<string, IEnumerable<string>> responseHeaders = null);

        Task<HttpResponseMessage> GetResponseMessageWithSSL(object value, Uri fullUrl, string authorizationScheme,
                                                        string authorizationValue, HttpMethod method = null,
                                                        TimeSpan timeout = default(TimeSpan),
                                                        Dictionary<string, IEnumerable<string>> requestHeaders = null,
                                                        Dictionary<string, IEnumerable<string>> responseHeaders = null);
    }


    public class HttpClientHelper : IHttpClientHelper
    {
        private const string ServiceCallHeader = "UrlReaderServiceCalls";
        private const string ContentType = "application/json";
        private const string logMessage = "Done with the external service call. Returning to the caller.";
        ILogger<HttpClientHelper> _logger;

        IHttpClientFactory _httpClientFactory;
        public HttpClientHelper(IHttpClientFactory httpClientFactory, ILogger<HttpClientHelper> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetResponseMessage(object value, Uri fullUrl, string authorizationScheme, string authorizationValue, HttpMethod method = null,
                                                              TimeSpan timeout = default(TimeSpan), Dictionary<string, IEnumerable<string>> requestHeaders = null,
                                                              Dictionary<string, IEnumerable<string>> responseHeaders = null)
        {
            HttpResponseMessage response = null;
            Dictionary<string, object> serviceCallHeaders = new Dictionary<string, object>();

            try
            {
                //Adding request header to dictionary for logging purpose
                if (requestHeaders != null)
                    requestHeaders.Where(h => h.Key.Contains(ServiceCallHeader)).Select(x => serviceCallHeaders[x.Key] = x.Value);

                var httpClient = _httpClientFactory.CreateClient("ServiceCall");


                httpClient.BaseAddress = fullUrl;

                if (requestHeaders != null)
                    requestHeaders.ToList().ForEach(x => httpClient.DefaultRequestHeaders.Add(x.Key, x.Value));

                var request = CreateHttpRequestMessage(value, method, fullUrl, authorizationScheme, authorizationValue);

                response = await httpClient.GetAsync(fullUrl);

                if (responseHeaders != null)
                    responseHeaders.Where(h => h.Key.Contains(ServiceCallHeader)).Select(x => serviceCallHeaders[x.Key] = x.Value);

                //Todo: Implement logger.
                _logger.LogDebug("Request completed: {route} {method} {code} {headers}", response.RequestMessage.RequestUri, response.RequestMessage.Method, response.StatusCode, response.Headers);

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message, null);
                throw;
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetResponseMessageWithSSL(object value, Uri fullUrl, string authorizationScheme, string authorizationValue, HttpMethod method = null,
                                                        TimeSpan timeout = default(TimeSpan), Dictionary<string, IEnumerable<string>> requestHeaders = null,
                                                        Dictionary<string, IEnumerable<string>> responseHeaders = null)
        {
            HttpResponseMessage response = null;
            Dictionary<string, object> serviceCallHeaders = new Dictionary<string, object>();

            try
            {
                //Adding request header to dictionary for logging purpose
                if (requestHeaders != null)
                    requestHeaders.Where(h => h.Key.Contains(ServiceCallHeader)).Select(x => serviceCallHeaders[x.Key] = x.Value);

                var httpClient = _httpClientFactory.CreateClient("ServiceCallWithSsl");

                httpClient.BaseAddress = fullUrl;

                if (requestHeaders != null)
                    requestHeaders.ToList().ForEach(x => httpClient.DefaultRequestHeaders.Add(x.Key, x.Value));

                using (var request = CreateHttpRequestMessage(value, method, fullUrl, authorizationScheme, authorizationValue))
                {
                    response = await httpClient.SendAsync(request);

                    if (responseHeaders != null)
                        responseHeaders.Where(h => h.Key.Contains(ServiceCallHeader)).Select(x => serviceCallHeaders[x.Key] = x.Value);

                    //Todo: Implement logger.
                    _logger.LogDebug("Request completed: {route} {method} {code} {headers}", response.RequestMessage.RequestUri, response.RequestMessage.Method, response.StatusCode, response.Headers);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message, null);
                throw;
            }

            return response;
        }

        private HttpRequestMessage CreateHttpRequestMessage(object value, HttpMethod httpMethod, Uri fullUrl, string authorizationScheme, string authorizationValue)
        {
            string jsonValue = JsonConvert.SerializeObject(value);
            var content = new StringContent(jsonValue);
            var header = new MediaTypeHeaderValue(ContentType);
            var acceptHeader = new MediaTypeWithQualityHeaderValue(ContentType);
            content.Headers.ContentType = header;

            if (httpMethod == null)
            {
                httpMethod = HttpMethod.Post;
            }

            HttpRequestMessage msg;
            msg = httpMethod == HttpMethod.Get ? new HttpRequestMessage(httpMethod, fullUrl) : new HttpRequestMessage(httpMethod, fullUrl) { Content = content };

            if (authorizationScheme != null)
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue(authorizationScheme, authorizationValue);
            }

            msg.Headers.Accept.Add(acceptHeader);

            return msg;
        }
    }
}
