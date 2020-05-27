using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MapDisplayApp.Proxies
{
    public static class HttpProxy
    {
        public static string DownloadResource(string uri)
        {
            return DownloadResource(new Uri(uri));
        }

        public static string DownloadResource(Uri uri)
        {
            string content = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(uri).Result)
                    {
                        using (HttpContent httpContent = response.Content)
                        {
                            content = httpContent.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception was thrown during downloading web page: {ex.Message} {ex.StackTrace}");
            }

            return content;
        }

    }
}
