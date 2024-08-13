using Newtonsoft.Json;

namespace basic_face_detection;

public class Response
{
    [JsonProperty("image_url")]
    public string ImageUrl {get; set;}
}