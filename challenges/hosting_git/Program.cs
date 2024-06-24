using System.Net;
using Newtonsoft.Json;

namespace hosting_git;

class Program
{
  static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }

    var problemUrl = $"https://hackattic.com/challenges/hosting_git/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/hosting_git/solve?access_token={args[0]}";

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

      bodyJson = resultContent.Result.Replace(" ", "");
    }

    var body = JsonConvert.DeserializeObject<Response>(bodyJson);

    Console.WriteLine($"{body.SSHkey}");
    Console.WriteLine($"{body.Username}");
    Console.WriteLine($"{body.RepoPath}");
    Console.WriteLine($"{body.PushToken}");
  }
}
