using Newtonsoft.Json;

namespace password_hashing;

public class Response
{
  [JsonProperty("password")]
  public string Password { get; set; }

  [JsonProperty("salt")]
  public string Salt { get; set; }

  [JsonProperty("pbkdf2")]
  public Pbkdf2 Pbkdf2 { get; set; }

  [JsonProperty("scrypt")]
  public Scrypt Scrypt { get; set; }
}

public class Pbkdf2
{
  [JsonProperty("hash")]
  public string Hash { get; set; }

  [JsonProperty("rounds")]
  public int Rounds { get; set; }
}

public class Scrypt
{
  [JsonProperty("N")]
  public int N { get; set; }

  [JsonProperty("p")]
  public int P { get; set; }

  [JsonProperty("r")]
  public int R { get; set; }

  [JsonProperty("buflen")]
  public int Buflen { get; set; }

  [JsonProperty("_control")]
  public string Control { get; set; }
}
