using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace websocket_chit_chat;

public class Listener
{
  public Task ListenTask { get; set; }
  public string Key { get; set; }
  private ClientWebSocket Socket { get; set; }

  public Listener(string url)
  {
    Socket = new ClientWebSocket();
    ListenTask = Listen(url);
    Key = string.Empty;
  }

  public Task Listen(string url)
  {
    return Task.Run(async () =>
    {
      var sw = new Stopwatch();
      try
      {
        long prev = 0;
        byte[] buffer = new byte[1024];
        await Socket.ConnectAsync(new Uri(url), CancellationToken.None);
        sw.Start();
        while (string.IsNullOrEmpty(Key))
        {
          var result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
          var data = Encoding.UTF8.GetString(buffer, 0, result.Count);

          Console.WriteLine(data);

          if (data == "ping!")
          {
            var ellapsed = sw.ElapsedMilliseconds;
            var getInterval = GetTimeInterval(sw.ElapsedMilliseconds - prev);
            prev = sw.ElapsedMilliseconds;

            var bytes = Encoding.UTF8.GetBytes(getInterval);
            var sendBuffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await Socket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
          }

          if (data.Contains("congratulations!"))
          {
            var quoteIndex = data.IndexOf("\"");
            var key = data.Substring(quoteIndex + 1, data.Length - quoteIndex - 2);
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            Key = key;
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        throw e;
      }
    }, CancellationToken.None);
  }

  // add a small buffer to account for latency
  private string GetTimeInterval(long milliseconds)
  {
    if (milliseconds >= 500 && milliseconds <= 900)
    {
      return "700";
    }
    else if (milliseconds >= 1300 && milliseconds <= 1700)
    {
      return "1500";
    }
    else if (milliseconds >= 1800 && milliseconds < 2200)
    {
      return "2000";
    }
    else if (milliseconds >= 2300 && milliseconds <= 2700)
    {
      return "2500";
    }

    return "3000";
  }
}
