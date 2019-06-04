using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExploringChannelsAndDataflow.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int requests = 100;
            using (var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") })
            {
                var ongoingRequests =
                    Enumerable
                        .Range(0, requests)
                        .Select(async i =>
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(50)); // just for the handlers to have time to pass the messages around
                        return await client.GetAsync($"submit?i={i}");
                        })
                        .ToList();

                var results = await Task.WhenAll(ongoingRequests);
                Console.WriteLine("In {0} requests, {1} failed, being {2} rejected.",
                    results.Length,
                    results.Count(r => r.StatusCode != HttpStatusCode.Accepted),
                    results.Count(r => r.StatusCode == HttpStatusCode.ServiceUnavailable));
            }
        }
    }
}
