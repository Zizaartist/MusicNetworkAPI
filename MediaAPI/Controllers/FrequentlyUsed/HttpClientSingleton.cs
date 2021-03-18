using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiClick.Controllers.FrequentlyUsed
{
    public class HttpClientSingleton
    {
        private static object lockObj = new object(); //Избегаем проблем от параллельных вызовов

        private static HttpClient httpClient;
        public static HttpClient HttpClient 
        {
            get 
            {
                lock (lockObj)
                {
                    if (HttpClient == null)
                    {
                        httpClient = new HttpClient();
                    }
                }
                return httpClient;
            }
        }
    }
}
