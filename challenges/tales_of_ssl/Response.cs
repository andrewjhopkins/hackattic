using Newtonsoft.Json;

namespace tales_of_ssl;

public class Response
{
  [JsonProperty("private_key")]
  public string PrivateKey { get; set; }

  [JsonProperty("required_data")]
  public RequiredData RequiredData { get; set; }
}

public class RequiredData
{
  [JsonProperty("domain")]
  public string Domain { get; set; }

  [JsonProperty("serial_number")]
  public string SerialNumber { get; set; }

  [JsonProperty("country")]
  public string Country { get; set; }
}
