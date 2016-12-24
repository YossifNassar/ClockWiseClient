using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TryClock.Logic
{
    public class ClockClient
    {
        static HttpClient client;

        public ClockClient()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://clockapi.azurewebsites.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Uri> CreateMetricAsync(Metric metric)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("metrics", metric);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        public static async Task<IEnumerable<Metric>> GetMetricsAsync(string path)
        {
            HttpClient client = new HttpClient();
            IEnumerable<Metric> m = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                m = await response.Content.ReadAsAsync<IEnumerable<Metric>>();
            }
            return m;
        }

    }
}
