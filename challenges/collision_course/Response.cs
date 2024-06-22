using Newtonsoft.Json;

namespace collision_course
{
    public class Response
    {
        [JsonProperty("include")]
        public string Include { get; set; }
    }
}
