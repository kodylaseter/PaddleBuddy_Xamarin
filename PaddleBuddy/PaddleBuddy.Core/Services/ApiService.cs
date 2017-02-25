using System;
using System.Threading.Tasks;
using Flurl.Http;
using PaddleBuddy.Core.Models;

namespace PaddleBuddy.Core.Services
{
    public abstract class ApiService : BaseCoreService
    {
        private const string ContentTypeJson = "application/json";
        
        public async Task<Response> PostAsync(string url, object data)
        {
            var response = new Response();

            if (NetworkService.IsServerAvailable)
            {
                var fullUrl = SysPrefs.ApiBase + url;
                try
                {
                    response = await fullUrl.WithHeader("ContentType", ContentTypeJson)
                        .PostJsonAsync(data).ReceiveJson<Response>();
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Problem reaching remote server");
                    LogService.ExceptionLog(e.Message);
                    response = new Response
                    {
                        Success = false
                    };
                }
            }
            else
            {
                response.Success = false;
                response.Detail = "No network connection";
                LogService.Log("No network connection available!");
            }
            return response;

        }

        public async Task<Response> GetAsync(string url)
        {
            var response = new Response();
            if (NetworkService.IsServerAvailable)
            {
                var fullUrl = SysPrefs.ApiBase + url;
                try
                {
                    
                    response = await fullUrl.GetJsonAsync<Response>();
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Problem reaching remote server");
                    LogService.ExceptionLog(e.Message);
                    response = new Response
                    {
                        Success = false
                    };
                }
            }
            else
            {
                response.Success = false;
                response.Detail = "No network connection";
                LogService.Log("No network connection available!");
            }
            return response;
        }

        public async Task<Response> GetAsync(string url, object multiple)
        {
            var response = new Response();
            if (NetworkService.IsServerAvailable)
            {
                var fullUrl = SysPrefs.ApiBase + url;
                try
                {
                    response = await fullUrl.WithHeaders(multiple).GetJsonAsync<Response>();
                }
                catch (Exception e)
                {
                    LogService.ExceptionLog("Problem reaching remote server");
                    LogService.ExceptionLog(e.Message);
                    response = new Response
                    {
                        Success = false
                    };
                }
            }
            else
            {
                response.Success = false;
                response.Detail = "No network connection";
                LogService.Log("No network connection available!");
            }

            return response;
        }

        public async Task<Response> GetAsync(string url, string name, string value)
        {
            //TODO: implement single header api get
            throw new NotImplementedException();
        }
    }
}
