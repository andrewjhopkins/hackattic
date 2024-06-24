using Newtonsoft.Json;

namespace hosting_git;

public class Response
{
  [JsonProperty("ssh_key")]
  public string SSHkey { get; set; }

  [JsonProperty("username")]
  public string Username { get; set; }

  [JsonProperty("repo_path")]
  public string RepoPath { get; set; }

  [JsonProperty("push_token")]
  public string PushToken { get; set; }
}
