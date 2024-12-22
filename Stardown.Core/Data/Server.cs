using System.Net;
using System.Net.WebSockets;

namespace Stardown.Core.Data;

public sealed class Server : IDisposable
{
    public string Address { get; private set; }
    public ushort Port { get; private set; }

    private object _syncLock = new object();

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private CookieContainer _cookies = new CookieContainer();

    private HttpClientHandler _httpHandler;
    private HttpClient _httpClient;

    private Queue<Guid> _updatedThreads = new Queue<Guid>();

    public Server(string address, ushort port)
    {
        Address = address;
        Port = port;

        _httpHandler = new HttpClientHandler()
        {
            CookieContainer = _cookies
        };
        
        _httpClient = new HttpClient(_httpHandler);
    }

    public Uri BaseUri
    {
        get
        {
            return new Uri($"https://{Address}:{Port}", UriKind.Absolute);
        }
    }

    public Uri ApiUri
    {
        get
        {
            return new Uri(BaseUri, "api");
        }
    }

    public Uri ThrUri
    {
        get
        {
            return new Uri(ApiUri, "thr");
        }
    }

    public Uri UsrUri
    {
        get
        {
            return new Uri(ApiUri, "usr");
        }
    }

    public Uri MsgUri
    {
        get
        {
            return new Uri(ApiUri, "msg");
        }
    }

    public Uri ConnectUri
    {
        get
        {
            return new Uri($"wss://{Address}:{Port}/api/connect", UriKind.Absolute);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _httpClient.Dispose();
    }

    public void ConnectHeartbeat()
    {
        Task.Run(() => Heartbeat(), _cts.Token);
    }

    public void SendMessage(Guid threadUuid, String contents, Guid? replyMessageUuid = null)
    {
        Task.Run(() =>
        {
            var formData = new Dictionary<string, string>();
            formData.Add("thread_uuid", threadUuid.ToString());
            formData.Add("contents", contents);

            // if (replyMessageUuid is not null)
            //     formData.Add("reply_message_uuid", replyMessageUuid.ToString());

            return PostForm(MsgUri, formData);
        });
    }

    private async Task PostForm(Uri uri, IEnumerable<KeyValuePair<string, string>> formData)
    {
        try
        {
            await _httpClient.PostAsync(MsgUri, new FormUrlEncodedContent(formData));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task Heartbeat()
    {
        using var handler = new SocketsHttpHandler() { CookieContainer = _cookies };
        using var heartbeat = new ClientWebSocket();

        try
        {
            await heartbeat.ConnectAsync(ConnectUri, new HttpMessageInvoker(handler), default);
            Console.WriteLine($"Connected to {ApiUri}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        while (!_cts.Token.IsCancellationRequested)
        {
            var bytes = new byte[17];
            await heartbeat.ReceiveAsync(bytes, default);

            lock (_syncLock)
            {
                _updatedThreads.Append(new Guid(bytes.AsSpan(1, 16)));
            }
        }

        await heartbeat.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
    }
}
