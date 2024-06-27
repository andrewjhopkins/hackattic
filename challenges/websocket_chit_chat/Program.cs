using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace websocket_chit_chat;

class Program
{
  static void Main(string[] args)
  {
    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }

    var problemUrl = $"https://hackattic.com/challenges/websocket_chit_chat/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/websocket_chit_chat/solve?access_token={args[0]}";

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

    var listener = new Listener($"wss://hackattic.com/_/ws/{body.Token}");
    Task.WaitAll(listener.ListenTask);

    var secret = listener.Key;
    Console.WriteLine($"Key found: {secret}");

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
