using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace miniminer;

class Program
{
  public static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }


    var problemUrl = $"https://hackattic.com/challenges/mini_miner/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/mini_miner/solve?access_token={args[0]}";

    string body;
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
      body = resultContent.Result.Replace(" ", "");
    }

    var difficultyIndex = body.IndexOf("\"difficulty\"") + "\"difficulty\":".Length;
    var difficultyString = "";

    while (char.IsNumber(body[difficultyIndex]))
    {
      difficultyString += body[difficultyIndex];
      difficultyIndex += 1;
    }

    var target = (int)1 << (256 - int.Parse(difficultyString));

    var dataIndex = body.IndexOf("\"data\"");
    var dataEndIndex = body.IndexOf("]]") + 2;

    var data = body.Substring(dataIndex, dataEndIndex - dataIndex);

    var nonce = 0;
    using (SHA256 sha256 = SHA256.Create())
    {
      while (true)
      {
        var payload = System.Text.Encoding.UTF8.GetBytes($"{{{data},\"nonce\":{nonce}}}");
        var hashValue = sha256.ComputeHash(payload);

        // theres probably a better way to do this
        // need to reverse the bits C# uses for ToInt32
        var hashIntValue = BitConverter.ToInt32(hashValue);
        var hashIntValueBytes = BitConverter.GetBytes(hashIntValue);
        Array.Reverse(hashIntValueBytes);
        hashIntValue = BitConverter.ToInt32(hashIntValueBytes);

        if (hashIntValue > 0 && hashIntValue < target)
        {
          Console.WriteLine($"nonce: {nonce}");

          using (var httpClient = new HttpClient())
          {
            var jsonBody = new { nonce = nonce };
            var jsonContent = JsonSerializer.Serialize(jsonBody);
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
          nonce += 1;
        }
      }
    }
  }
}


