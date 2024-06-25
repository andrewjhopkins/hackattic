using Newtonsoft.Json;
using System.Diagnostics;
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

        var filePath = Path.Combine(Path.GetTempPath(), "zipped.zip");

        using (var client = new HttpClient())
        {
            var response = client.GetAsync(body.ZipUrl);
            Task.WaitAll(response);

            using (var streamToReadFrom = response.Result.Content.ReadAsStream())
            {
                using (var fs = new FileStream(filePath, FileMode.CreateNew))
                {
                    streamToReadFrom.CopyTo(fs);
                }
            }
        }

        var process = new Process();

        var startInfo = new ProcessStartInfo
        {
            FileName = "./crack.sh",
            UseShellExecute = true,
        };

        process.StartInfo = startInfo;

        process.Start();
        var wait = process.WaitForExitAsync();
        Task.WaitAll(wait);


        var unzippedFilePath = Path.Combine(Path.GetTempPath(), "zipped/secret.txt");
        var bytes = File.ReadAllBytes(unzippedFilePath);

        var secret = System.Text.Encoding.UTF8.GetString(bytes).Trim();

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
