using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace password_hashing;

class Program
{
  public static void Main(string[] args)
  {

    if (args.Count() < 1)
    {
      Console.WriteLine("Usage: dotnet run <access token>");
      return;
    }


    var problemUrl = $"https://hackattic.com/challenges/password_hashing/problem?access_token={args[0]}";
    var solveUrl = $"https://hackattic.com/challenges/password_hashing/solve?access_token={args[0]}";

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

    Console.WriteLine($"body: {bodyJson}");
    var body = JsonConvert.DeserializeObject<Response>(bodyJson);

    var sha256 = string.Empty;
    var hmac = string.Empty;
    var pbkdf2 = string.Empty;
    var scrypt = string.Empty;

    var passwordBytes = Encoding.UTF8.GetBytes(body.Password);

    // Generate sha256
    using (var sha256Client = SHA256.Create())
    {
      var sha256Hash = sha256Client.ComputeHash(passwordBytes);
      sha256 = BitConverter.ToString(sha256Hash).Replace("-", "").ToLower();
    }

    var saltBytes = System.Convert.FromBase64String(body.Salt);

    // Generate hmac-sha256
    using (var hmacsha256Client = new HMACSHA256(saltBytes))
    {
      var hmacHash = hmacsha256Client.ComputeHash(passwordBytes);
      hmac = BitConverter.ToString(hmacHash).Replace("-", "").ToLower();
    }

    // Generate pbkdf2
    var sha256ByteCount = 256 / 8;
    var pbkdf2Bytes = new Rfc2898DeriveBytes(passwordBytes, saltBytes, body.Pbkdf2.Rounds, HashAlgorithmName.SHA256).GetBytes(sha256ByteCount);
    pbkdf2 = BitConverter.ToString(pbkdf2Bytes).Replace("-", "").ToLower();

    // Generate Scrypt
    // var scryptClient = new ScryptEncoder(iterationCount: body.Scrypt.N, blockSize: body.Scrypt.R, threadCount: body.Scrypt.P);
    var scryptHash = ScryptEncoder.CryptoScrypt(passwordBytes, saltBytes, body.Scrypt.N, body.Scrypt.R, body.Scrypt.P);
    scrypt = BitConverter.ToString(scryptHash).Replace("-", "").ToLower();

    Console.WriteLine($"sha256: {sha256}");
    Console.WriteLine($"hmac: {hmac}");
    Console.WriteLine($"pbkdf2: {pbkdf2}");
    Console.WriteLine($"scrypt: {scrypt}");

    using (var httpClient = new HttpClient())
    {
      var jsonBody = new { sha256 = sha256, hmac = hmac, pbkdf2 = pbkdf2, scrypt = scrypt, };
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

  private string HexDigest(byte[] input)
  {
    return BitConverter.ToString(input).Replace("-", "").ToLower();
  }
}
