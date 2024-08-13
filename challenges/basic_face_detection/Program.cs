using System.Drawing;
using System.Net;
using Newtonsoft.Json;

namespace basic_face_detection;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Count() < 1)
        {
            Console.WriteLine("Usage: dotnet run <access token>");
            return;
        }

        var problemUrl = $"https://hackattic.com/challenges/basic_face_detection/problem?access_token={args[0]}";
        var solveUrl = $"https://hackattic.com/challenges/basic_face_detection/solve?access_token={args[0]}";

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

        var tempFilePath = Path.Combine(Path.GetTempPath(), "detection_image.jpg");

        using (var httpClient = new HttpClient())
        {
            var imageContent = httpClient.GetByteArrayAsync(body.ImageUrl);
            Task.WaitAll(imageContent);
            File.WriteAllBytes(tempFilePath, imageContent.Result);
        }

        var image = Image.FromFile(tempFilePath);
    }
}