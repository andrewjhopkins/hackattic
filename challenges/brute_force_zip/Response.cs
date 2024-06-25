using Newtonsoft.Json;

namespace brute_force_zip;

public class Response
{
    [JsonProperty("zip_url")]
    public string ZipUrl { get; set; }
}
