using Newtonsoft.Json;

namespace jotting_jwts
{
    public class Response
    {
        [JsonProperty("jwt_secret")]
        public string JwtSecret { get; set; }
    }
}
