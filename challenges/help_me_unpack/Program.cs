using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace help_me_unpack;

class Program
{
  public static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }


    var problemUrl = $"https://hackattic.com/challenges/help_me_unpack/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/help_me_unpack/solve?access_token={args[0]}";

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

    var startIndex = body.IndexOf(":") + 2;
    var length = body.Count() - 2 - startIndex;

    var base64String = body.Substring(startIndex, length);

    var bytes = System.Convert.FromBase64String(base64String);
    var offset = 0;

    var intValue = BitConverter.ToInt32(bytes, offset);
    offset += sizeof(Int32);

    // uint
    var uintValue = BitConverter.ToUInt32(bytes, offset);
    offset += sizeof(UInt32);

    // short
    var shortValue = BitConverter.ToInt16(bytes, offset);
    offset += 4; // Bugged here? Answer expects us to skip 4 bytes even though shorts use 2

    // float
    var floatValue = BitConverter.ToSingle(bytes, offset);
    offset += sizeof(float);

    // double
    var doubleValue = BitConverter.ToDouble(bytes, offset);
    offset += sizeof(double);

    // big_endian_double
    var bedValueBytes = bytes.Skip(offset).Take(sizeof(double)).ToArray();
    Array.Reverse(bedValueBytes);
    double bedValue = BitConverter.ToDouble(bedValueBytes);

    using (var httpClient = new HttpClient())
    {
      var jsonBody = new { @int = intValue, @uint = uintValue, @short = shortValue, @float = floatValue, @double = doubleValue, big_endian_double = bedValue };
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
}
