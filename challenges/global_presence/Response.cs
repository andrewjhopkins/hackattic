using Newtonsoft.Json;

namespace brute_force_zip;

public class Response
{
    [JsonProperty("presence_token")]
    public string PresenceToken { get; set; }
}
