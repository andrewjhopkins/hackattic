using System.Diagnostics;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace tales_of_ssl;

class Program
{
  public static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }


    var problemUrl = $"https://hackattic.com/challenges/tales_of_ssl/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/tales_of_ssl/solve?access_token={args[0]}";

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

    // Didn't want to add everything here. Only adding when needed
    var countryCodeConvert = new Dictionary<string, string>();
    countryCodeConvert.Add("KeelingIslands", "CC");
    countryCodeConvert.Add("ChristmasIsland", "CX");
    countryCodeConvert.Add("CocosIslands", "CC");
    countryCodeConvert.Add("TokelauIslands", "TK");
    countryCodeConvert.Add("SintMaarten", "SX");

    var body = JsonConvert.DeserializeObject<Response>(bodyJson);

    var countryCode = countryCodeConvert.GetValueOrDefault(body.RequiredData.Country);

    if (string.IsNullOrEmpty(countryCode))
    {
      Console.WriteLine($"Country {body.RequiredData.Country} not in dictionary");
      return;
    }

    var privateKey = body.PrivateKey;
    var keyFilePath = Path.Combine(Path.GetTempPath(), "key.pem");

    using (StreamWriter sw = new StreamWriter(keyFilePath))
    {
      sw.WriteLine("-----BEGIN PRIVATE KEY-----");
      sw.WriteLine(privateKey);
      sw.WriteLine("-----END PRIVATE KEY-----");
    }

    var certFile = Path.Combine(Path.GetTempPath(), "req.der");

    GenerateCert(certFile, body.RequiredData.Domain, body.RequiredData.SerialNumber, countryCode);

    var fileBytes = File.ReadAllBytes(certFile);
    var base64File = System.Convert.ToBase64String(fileBytes);

    Console.WriteLine($"base64File: {base64File}");

    using (var httpClient = new HttpClient())
    {
      var jsonBody = new { certificate = base64File };
      var jsonContent = System.Text.Json.JsonSerializer.Serialize(jsonBody);
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

  private static string GenerateCert(string certFilePath, string domain, string serialNumber, string countryCode)
  {
    var process = new Process();
    var arguments = $"req -outform der -config /etc/ssl/openssl.cnf -key /tmp/key.pem -new -x509 -days 7300 -sha256 -extensions v3_ca -out {certFilePath} -set_serial {serialNumber} -subj \"/C={countryCode}/CN={domain}\"";


    var startInfo = new ProcessStartInfo
    {
      FileName = "/usr/bin/openssl",
      Arguments = arguments,

      UseShellExecute = false,
      RedirectStandardOutput = true,
    };


    process.StartInfo = startInfo;

    process.Start();
    var output = process.StandardOutput.ReadToEndAsync();
    Task.WaitAll(output);
    var wait = process.WaitForExitAsync();
    Task.WaitAll(wait);
    return output.Result;
  }
}
