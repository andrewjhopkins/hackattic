using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace collision_course;

class Program
{
    static void Main(string[] args)
    {
        if (args.Count() < 1)
        {
            Console.WriteLine("Usage: dotnet run <access token>");
            return;
        }

        var problemUrl = $"https://hackattic.com/challenges/collision_course/problem?access_token={args[0]}";
        var solveUrl = $"https://hackattic.com/challenges/collision_course/solve?access_token={args[0]}";

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

        var includeString = JsonConvert.DeserializeObject<Response>(bodyJson)?.Include;

        if (string.IsNullOrEmpty(includeString)) 
        {
            Console.WriteLine("Include not found in body");
            return;
        }

        var includeBytes = Encoding.UTF8.GetBytes(includeString);

        // from https://www.codeproject.com/Articles/11643/Exploiting-MD5-collisions-in-C
        // if Md5(x) == Md5(y) then Md5(x + q) == Md5(y + q)
        // just need to use known collision and append the include string

        var file1 = new List<byte> 
        { 
            0xd1, 0x31, 0xdd, 0x02, 0xc5, 0xe6
           , 0xee , 0xc4 , 0x69 , 0x3d, 0x9a , 0x06
           , 0x98 , 0xaf , 0xf9 , 0x5c , 0x2f , 0xca
           , 0xb5 , /**/0x87 , 0x12 , 0x46 , 0x7e
           , 0xab , 0x40 , 0x04 , 0x58 , 0x3e , 0xb8
           , 0xfb , 0x7f , 0x89 , 0x55 , 0xad
           , 0x34 , 0x06 , 0x09 , 0xf4 , 0xb3 , 0x02
           , 0x83 , 0xe4 , 0x88 , 0x83 , 0x25
           , 0x71 , 0x41 , 0x5a, 0x08 , 0x51 , 0x25
           , 0xe8 , 0xf7 , 0xcd , 0xc9 , 0x9f ,
           0xd9 , 0x1d , 0xbd , 0xf2 , 0x80 , 0x37
           , 0x3c , 0x5b , 0xd8 , 0x82 , 0x3e
           , 0x31 , 0x56 , 0x34 , 0x8f , 0x5b , 0xae
           , 0x6d , 0xac , 0xd4 , 0x36 , 0xc9
           , 0x19 , 0xc6 , 0xdd , 0x53 , 0xe2 , 0xb4
           , 0x87 , 0xda , 0x03 , 0xfd , 0x02
           , 0x39 , 0x63 , 0x06 , 0xd2 , 0x48 , 0xcd
           , 0xa0 , 0xe9 , 0x9f , 0x33 , 0x42
           , 0x0f , 0x57 , 0x7e , 0xe8 , 0xce , 0x54
           , 0xb6 , 0x70 , 0x80 , 0xa8 , 0x0d
           , 0x1e , 0xc6 , 0x98 , 0x21 , 0xbc , 0xb6
           , 0xa8 , 0x83 , 0x93 , 0x96 , 0xf9
           , 0x65 , 0x2b , 0x6f , 0xf7 , 0x2a , 0x70
        };

        var file2 = new List<byte> 
        {
           0xd1 , 0x31, 0xdd , 0x02 , 0xc5 , 0xe6
           , 0xee , 0xc4 , 0x69 , 0x3d , 0x9a , 0x06
           , 0x98 , 0xaf , 0xf9 , 0x5c, 0x2f , 0xca
           , 0xb5 , /**/0x07 , 0x12 , 0x46 , 0x7e
           , 0xab , 0x40 , 0x04 , 0x58 , 0x3e , 0xb8
           , 0xfb , 0x7f , 0x89 , 0x55 , 0xad
           , 0x34 , 0x06 , 0x09 , 0xf4 , 0xb3 , 0x02
           , 0x83 , 0xe4 , 0x88 , 0x83 , 0x25
           ,/**/ 0xf1 , 0x41 , 0x5a , 0x08 , 0x51 , 0x25
           , 0xe8 , 0xf7 , 0xcd , 0xc9 , 0x9f
           , 0xd9 , 0x1d , 0xbd , /**/0x72 , 0x80
           , 0x37 , 0x3c , 0x5b, 0xd8 , 0x82
           , 0x3e , 0x31 , 0x56 , 0x34 , 0x8f , 0x5b
           , 0xae , 0x6d , 0xac , 0xd4 , 0x36
           , 0xc9 , 0x19 , 0xc6 , 0xdd , 0x53 , 0xe2
           , /**/0x34 , 0x87 , 0xda , 0x03 , 0xfd
           , 0x02 , 0x39 , 0x63 , 0x06 , 0xd2 , 0x48
           , 0xcd , 0xa0, 0xe9 , 0x9f , 0x33
           , 0x42 , 0x0f , 0x57 , 0x7e , 0xe8 , 0xce
           , 0x54 , 0xb6 , 0x70 , 0x80 , /**/ 0x28
           , 0x0d , 0x1e, 0xc6 , 0x98 , 0x21 , 0xbc
           , 0xb6 , 0xa8 , 0x83 , 0x93 , 0x96
           , 0xf9 , 0x65 , /* flag byte*/0xab
           , 0x6f , 0xf7 , 0x2a , 0x70
        };

        file1.AddRange(includeBytes);
        var file1Array = file1.ToArray();

        file2.AddRange(includeBytes);
        var file2Array = file2.ToArray();

        string file1HashBase64;
        string file2HashBase64;

        using (var md5Client = MD5.Create())
        {
            var file1Hash = md5Client.ComputeHash(file1Array);
            var file2Hash = md5Client.ComputeHash(file2Array);

            file1HashBase64 = Convert.ToBase64String(file1Hash);
            file2HashBase64 = Convert.ToBase64String(file2Hash);
        }

        if (file1HashBase64 == file2HashBase64)
        {
            var file1Base64 = Convert.ToBase64String(file1Array);
            var file2Base64 = Convert.ToBase64String(file2Array);

            if (file1Base64 == file2Base64)
            {
                Console.WriteLine("Files are the same");
                return;
            }
            using (var httpClient = new HttpClient())
            {
                var jsonBody = new { files = new[] { file1Base64, file2Base64 } };
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
        else
        {
            Console.WriteLine("Md5 hashes do not match");
            return;
        }
    }
}
