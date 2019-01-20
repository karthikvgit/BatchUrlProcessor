using Common.Utilities.Constants;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Utilities.Handlers
{
    public class CorrelationIdHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(HttpHeaderNames.UrlReaderCorrelationIdHeader))
            {
                var missingHeader = @"You must supply a header called" + HttpHeaderNames.UrlReaderCorrelationIdHeader + ". " +
                    "Please verify if you are missing the header or if you are spelling the " + HttpHeaderNames.UrlReaderCorrelationIdHeader + " right.";
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(missingHeader)
                };
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
