using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace GLibrary.Device.Net
{
    /// <summary>
    /// Under development
    /// </summary>
    public class HttpClient:CommonBase 
    {
        private Windows.Web.Http.HttpClient Client;

        public HttpClient() { Client = new Windows.Web.Http.HttpClient(); }

        /// <summary>
        /// Strongly recommended use this methord. Just because it's simple, you can focus on the application function.
        /// </summary>
        /// <param name="Uri">Server url and params build</param>
        /// <returns></returns>
        public async Task<string> Get(string Uri)
        {
            if (Client == null) return string.Empty;

            var response = await Client.GetStringAsync(new Uri(Uri));
            return response;
        }


        public async void Post(string Uri,Dictionary<string,string> paras)
        {
            HttpMultipartFormDataContent content = new HttpMultipartFormDataContent();
            foreach (var item in paras)
            {
                content.Add(new HttpStringContent(item.Value), item.Key);
            }
            var a= await Client.PostAsync(new Uri(""), content);
            var response=await Client.PostAsync(new Uri(Uri), new HttpMultipartFormDataContent());
        }

        public override void Dispose()
        {
            Client.Dispose();
        }

        /// <summary>
        /// Helper Methord
        /// </summary>
        /// <param name="BaseUrl">Base Url</param>
        /// <param name="paras">parameters</param>
        /// <returns>Uri string</returns>
        public static string UriBuilder(string BaseUrl,Dictionary<string,string> paras)
        {
            StringBuilder sb = new StringBuilder(BaseUrl);
            sb.Append("?");
            foreach (var item in paras)
            {
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value);
                sb.Append("&");
            }
            //cut the last '&'
            return sb.ToString(0,sb.Length-1);
        }
    }
}
