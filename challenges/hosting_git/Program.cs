using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace hosting_git;

class Program
{
  static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token> or dotnet run <access token> <secret>");
      return;
    }

    var problemUrl = $"https://hackattic.com/challenges/hosting_git/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/hosting_git/solve?access_token={args[0]}";

    if (args.Count() < 2)
    {
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

        bodyJson = resultContent.Result;
      }

      var body = JsonConvert.DeserializeObject<Response>(bodyJson);

      var exportToBash = $"SSHKEY='{body.SSHkey}'; USERNAME={body.Username}; REPOPATH={body.RepoPath}; PUSHTOKEN={body.PushToken}";

      Console.WriteLine(exportToBash);

      return;
    }
    else
    {
      var secret = args[1];
      using (var httpClient = new HttpClient())
      {
          var jsonBody = new { secret = secret };
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
  }
}
