﻿using BatchUrlProcessor.Models;
using Common.Utilities.Constants;
using Common.Utilities.Helpers;
using HtmlAgilityPack;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BatchUrlProcessor.Helpers
{
    public interface IUrlReader
    {
        Task<UrlResponse> ReadUrlAsync(string url);
    }

    public class UrlReader : IUrlReader
    {
        IHttpClientHelper _httpClientHelper;
        public UrlReader(IHttpClientHelper httpClientHelper)
        {
            _httpClientHelper = httpClientHelper;
        }

        public async Task<UrlResponse> ReadUrlAsync(string url)
        {
            UrlResponse urlResponse = new UrlResponse();
            urlResponse.Url = url;
            bool isValidUrl = ValidateUrl(url);
            if (!isValidUrl)
            {
                urlResponse.statusCode = System.Net.HttpStatusCode.BadRequest;
                urlResponse.Error = new Error() { Code = "VE1001", Message = "The Url is not well formed. Please provide a valid url" };
                return urlResponse;
            }

            try
            {
                var requestHeaders = new Dictionary<string, IEnumerable<string>>();
                requestHeaders.Add(HttpHeaderNames.UrlReaderCorrelationIdHeader, new List<string> { GetCorrealtionId() });
                HttpResponseMessage httpResponse = null;
                using (DogStatsd.StartTimer("UrlReader.ReadUrlAsync"))
                {
                    httpResponse = await _httpClientHelper.GetResponseMessage(null, new Uri(url), null, null, HttpMethod.Get, default(TimeSpan), requestHeaders);
                }
                if (httpResponse?.Content?.Headers?.ContentType?.MediaType == "text/html")
                {
                    HtmlDocument pageDocument = new HtmlDocument();
                    pageDocument.LoadHtml(await httpResponse.Content.ReadAsStringAsync());
                    urlResponse.Title = pageDocument.DocumentNode.SelectSingleNode("//head/title")?.InnerText;
                }

                urlResponse.statusCode = httpResponse == null ? System.Net.HttpStatusCode.BadGateway : httpResponse.StatusCode;
                if (url.StartsWith("https"))
                {
                    var sslResponse = await RetryWithSsl(url);
                    urlResponse.Protocol = sslResponse.Error != null ? Protocol.TLS_Only : Protocol.SSL_and_TLS;
                }
                else
                    urlResponse.Protocol = Protocol.None;
            }
            catch (HttpRequestException httpex)
            {
                //Todo: log error once implemented
                urlResponse = await RetryWithSsl(url);
            }
            catch (Exception ex)
            {
                urlResponse.Error = new Error() { Code = "GE1001", Message = "An error occured. Please contact the administrator for more information." + ex.Message };
            }

            return urlResponse;
        }


        //Todo: This is not efficient. Have to find a better way to determine if the server supports TLS only
        //This should just be used as fall back if we want to support Ssl or not used at all if decided not to support (My reccomendation) as this is not considered secure anymore
        public async Task<UrlResponse> RetryWithSsl(string url)
        {

            UrlResponse urlResponse = new UrlResponse();
            urlResponse.Url = url;

            try
            {
                var requestHeaders = new Dictionary<string, IEnumerable<string>>();
                requestHeaders.Add(HttpHeaderNames.UrlReaderCorrelationIdHeader, new List<string> { GetCorrealtionId() });
                HttpResponseMessage httpResponse = null;
                using (DogStatsd.StartTimer("UrlReader.ReadUrlAsync.Ssl"))
                {
                    httpResponse = await _httpClientHelper.GetResponseMessageWithSSL(null, new Uri(url), null, null, HttpMethod.Get, default(TimeSpan), requestHeaders);
                }

                if (httpResponse?.Content?.Headers?.ContentType?.MediaType == "text/html")
                {
                    HtmlDocument pageDocument = new HtmlDocument();
                    pageDocument.LoadHtml(await httpResponse.Content.ReadAsStringAsync());
                    urlResponse.Title = pageDocument.DocumentNode.SelectSingleNode("//head/title")?.InnerText;
                }

                urlResponse.statusCode = httpResponse == null ? System.Net.HttpStatusCode.BadGateway : httpResponse.StatusCode;
            }
            catch (HttpRequestException httpex)
            {
                urlResponse.Error = new Error() { Code = "HE1001", Message = httpex.Message };
            }
            catch (Exception ex)
            {
                urlResponse.Error = new Error() { Code = "GE1001", Message = "An error occured. Please contact the administrator for more information." + ex.Message };
            }

            return urlResponse;
        }

        private bool ValidateUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public string GetCorrealtionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}