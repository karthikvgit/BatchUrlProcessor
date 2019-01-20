using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BatchUrlProcessor.Helpers;
using BatchUrlProcessor.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StatsdClient;

namespace BatchUrlProcessor.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessBatchController : ControllerBase
    {
        private IUrlReader _reader;

        public ProcessBatchController(IUrlReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// This is a post request accecpting list of urls and returning the captured details of the urls
        /// The url is expected to be absolute e.g https://google.com
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Response> Post([FromBody] List<string> urls)
        {
            DogStatsd.Increment("UrlReader.Requests.Count");
            Response response = new Response();
            var urlsProcessed = urls.Distinct().ToList();
            try
            {
                List<Task<UrlResponse>> tasks = new List<Task<UrlResponse>>();
                foreach (var url in urlsProcessed)
                {
                    DogStatsd.Increment("UrlReader.TotalUrls.Count");
                    using (DogStatsd.StartTimer("UrlReader.ReadUrlAsync"))
                    {
                        tasks.Add(_reader.ReadUrlAsync(url));
                    }
                }

                await Task.WhenAll(tasks);
                response.urlResponses = tasks.Select(t => t.Result).ToList(); 
                 
            }
            catch (System.Exception ex)
            {
                //Todo: log the error to a file or Elk
                response.Error = new Error() { Code = "GE1001", Message = "An unknown error occured when trying to fetch the urls" }; 
            }

            return response;
        }
    }
}