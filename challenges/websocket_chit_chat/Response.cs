using Newtonsoft.Json;

namespace websocket_chit_chat;

public class Response
{
  [JsonProperty("token")]
  public string Token { get; set; }
}


