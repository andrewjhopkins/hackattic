using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

// Usage dotnet run <token> false; ./restore.sh; dotnet run <token> true;

namespace backup_restore;

class Program
{
    static void Main(string[] args)
    {
        if (args.Count() < 2)
        {
        Console.WriteLine("Usage: dotnet run <access token> <submit>");
        return;
        }

        var submit = bool.Parse(args[1]);

        var problemUrl = $"https://hackattic.com/challenges/backup_restore/problem?access_token={args[0]}";
        var solveUrl = $"https://hackattic.com/challenges/backup_restore/solve?access_token={args[0]}";

        if(!submit)
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
                bodyJson = resultContent.Result.Replace(" ", "");
            }

            var dump = JsonConvert.DeserializeObject<Response>(bodyJson)?.Dump;
            if (string.IsNullOrEmpty(dump))
            {
                Console.WriteLine("Dump is null or empty.");
                return;
            }

            var dumpBytes = Convert.FromBase64String(dump);

            var dumpFilePath = Path.Combine(Path.GetTempPath(), "dump.sql.gz");

            using (FileStream fs = new FileStream(dumpFilePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(dumpBytes, 0, dumpBytes.Length);
            }

            // Run ./restore.sh here
            // Too much effort to do this completely in code 
        }

        else
        {
            var fileLines = new List<string>();

            var path = Path.Combine(Path.GetTempPath(), "ssn.txt");

            using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using(StreamReader sr = new StreamReader(fs))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        fileLines.Add(line);
                    }
                }
            }

            var ssns = fileLines.Where(x => x.Length == 11).ToArray();

            using (var httpClient = new HttpClient())
            {
                var jsonBody = new { alive_ssns = ssns };
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
