using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace jotting_jwts.Controllers
{

    [ApiController]
    [Route("")]
    public class Controller : ControllerBase
    {
        private IMemoryCache _memoryCache;
        private string Token { get; set; } = string.Empty;

        public Controller(IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache;
        }

        [HttpPost]
        [Route("start/{token}")]
        public async Task<IActionResult> StartAsync([FromRoute] string token)
        {
            Token = token;
            var problemUrl = $"https://hackattic.com/challenges/jotting_jwts/problem?access_token={Token}";
            var solveUrl = $"https://hackattic.com/challenges/jotting_jwts/solve?access_token={Token}";

            using (var httpClient = new HttpClient())
            {
                var responseGet = await httpClient.GetAsync(problemUrl);

                if (responseGet.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Access token is not valid.");
                    return BadRequest();
                }

                var bodyJson = await responseGet.Content.ReadAsStringAsync();
                var body = JsonConvert.DeserializeObject<Response>(bodyJson);

                _memoryCache.Set("JwtSecret", body.JwtSecret);

                var jsonBody = new { app_url = "https://jwtjot.azurewebsites.net/" };
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(jsonBody);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
                var responsePost = await httpClient.PostAsync(solveUrl, stringContent);

                if (responsePost.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Submission is not valid.");
                    return BadRequest();
                }

                return Ok();
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CollectAsync()
        {
            string jwtString;

            using (var memoryStream = new MemoryStream())
            {
                await Request.Body.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                jwtString = Encoding.UTF8.GetString(bytes);
            }

            if (string.IsNullOrEmpty(jwtString))
            {
                return NotFound();
            }
            else
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var jwtToken = tokenHandler.ReadJwtToken(jwtString);
                var rawSignature = jwtToken.RawSignature;
                var signatureBytes = Encoding.UTF8.GetBytes(jwtToken.EncodedHeader + "." + jwtToken.EncodedPayload);

                string jwtSecret;
                if (!_memoryCache.TryGetValue("JwtSecret", out jwtSecret))
                {
                    return StatusCode(500);
                }

                var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

                string signature;
                using (var hmacsha256Client = new HMACSHA256(keyBytes))
                {
                    var hmacHash = hmacsha256Client.ComputeHash(signatureBytes);
                    signature = System.Convert.ToBase64String(hmacHash).TrimEnd('=').Replace('+', '-').Replace('/', '_'); 
                }

                var validFrom = (jwtToken.ValidFrom == default || jwtToken.ValidFrom <= DateTime.Now);
                var validTo = (jwtToken.ValidTo == default || jwtToken.ValidTo >= DateTime.Now);

                if (signature == rawSignature && validFrom && validTo)
                {
                    var append = jwtToken.Payload.Claims.FirstOrDefault(x => x.Type == "append")?.Value;

                    if (!string.IsNullOrEmpty(append))
                    {
                        List<string> appends;
                        if (!_memoryCache.TryGetValue("Appends", out appends))
                        {
                            appends = new List<string>() { append };
                        }
                        else
                        {
                            appends.Add(append);
                        }

                        _memoryCache.Set("Appends", appends);

                        return NoContent();
                    }
                    else
                    {
                        List<string> appends = (List<string>)(_memoryCache.Get("Appends") ?? new List<string>());
                        var stringBuilder = new StringBuilder();

                        foreach (var a in appends)
                        {
                            stringBuilder.Append(a);
                        }

                        var answer = stringBuilder.ToString();

                        Response.Headers.ContentType = "application/json";
                        return Ok(new { solution = answer });
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
        }
    }
}
