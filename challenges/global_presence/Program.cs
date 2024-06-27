using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace brute_force_zip;

class Program
{
    static void Main(string[] args)
    {
        if (args.Count() < 1)
        {
            Console.WriteLine("Usage: dotnet run <access token>");
            return;
        }

        var problemUrl = $"https://hackattic.com/challenges/a_global_presence/problem?access_token={args[0]}";
        var solveUrl = $"https://hackattic.com/challenges/a_global_presence/solve?access_token={args[0]}";

        string bodyJson;
        using (var httpClient = new HttpClient())
        {
            var response = httpClient.GetAsync(problemUrl);
            Task.WaitAll(response);

            if (response.Result.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Access token is not valid.");
                return;
            }

            var resultContent = response.Result.Content.ReadAsStringAsync();
            Task.WaitAll(resultContent);

            bodyJson = resultContent.Result.Trim();
        }

        var body = JsonConvert.DeserializeObject<Response>(bodyJson);
        Console.WriteLine(body.PresenceToken);

        // Spun up 6 non US Azure VMs and configured them to proxy to https://hackattic.com with Nginx
        var proxyUrls = new[] {
            "<proxy ip1>",
            "<proxy ip2>",
            "<proxy ip3>",
            "<proxy ip4>",
            "<proxy ip5>",
            "<proxy ip6>",
        };

        var client = new HttpClient();
        var requests = proxyUrls.Select(url => client.GetAsync($"{url}/_/presence/{body.PresenceToken}")).ToList();
        Task.WhenAll(requests);

        foreach (var request in requests)
        {
            Console.WriteLine(request.Result.StatusCode);
        }

        // Call from US
        var url = $"https://hackattic.com/_/presence/{body.PresenceToken}";
        using (var httpClient = new HttpClient())
        {
            var response = httpClient.GetAsync(url);
            Task.WaitAll(response);

            if (response.Result.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Something went wrong: Status Code: {response.Result.StatusCode}");
                return;
            }

            var resultContent = response.Result.Content.ReadAsStringAsync();
            Task.WaitAll(resultContent);

            bodyJson = resultContent.Result.Trim();
        }

        Console.WriteLine(bodyJson);

        if (bodyJson.Length >= 20)
        {
            using (var httpClient = new HttpClient())
            {
                var jsonBody = new { };
                var jsonContent = JsonConvert.SerializeObject(jsonBody);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

                var postRequest = httpClient.PostAsync(solveUrl, stringContent);
                Task.WaitAll(postRequest);

                if (postRequest.Result.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Request was not accepted: ${postRequest.Result.StatusCode}");
                    return;
                }

                var responseString = postRequest.Result.Content.ReadAsStringAsync();
                Task.WaitAll(responseString);

                Console.WriteLine(responseString.Result);
                return;
            }
        }
        else
        {
            Console.WriteLine($"At least 7 countries not returned: {bodyJson}");
        }

    }
}
