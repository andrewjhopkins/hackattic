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

    var problemUrl = $"https://hackattic.com/challenges/brute_force_zip/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/brute_force_zip/solve?access_token={args[0]}";

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
    Console.WriteLine(body.ZipUrl);

  }
}
