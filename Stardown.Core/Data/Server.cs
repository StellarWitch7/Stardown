using System.Net;
using System.Net.WebSockets;
using PeterO.Cbor;

namespace Stardown.Core.Data;

public sealed class Server : IDisposable
{
    public string Username { get; private set; }
    public string Address { get; private set; }
    public ushort Port { get; private set; }

    private object _heartLock = new object();
    private bool _heartBeating = false;

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private CookieContainer _cookies = new CookieContainer();

    private HttpClientHandler _httpHandler;
    private HttpClient _httpClient;

    private Func<string, Task<string>> _getPassword = null!;
    private Func<Server, Message, Task> _onMessageReceived = null!;

    public Server(string username, string address, ushort port)
    {
        Username = username;
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

    public Uri AuthUri
    {
        get
        {
            return new Uri(BaseUri, "auth");
        }
    }

    public Uri HeartUri
    {
        get
        {
            return new Uri($"wss://{Address}:{Port}/api/heart", UriKind.Absolute);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _httpClient.Dispose();
    }

    public void ConnectHeartbeat(Func<string, Task<string>> getPassword, Func<Server, Message, Task> onMessageReceived)
    {
        lock (_heartLock) {
            _getPassword = getPassword;
            _onMessageReceived = onMessageReceived;

            if (!_heartBeating)
            {
                _heartBeating = true;
                Task.Run(() => Heartbeat(), _cts.Token);
            }
        }
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
            await _httpClient.PostAsync(uri, new FormUrlEncodedContent(formData));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //TODO: better error handling
        }
    }

    private async Task Heartbeat()
    {
        using var handler = new SocketsHttpHandler() { UseCookies = true, CookieContainer = _cookies };
        var heartbeat = new ClientWebSocket(); // manually disposed of
        heartbeat.Options.CollectHttpResponseDetails = true;

        try
        {
            Console.WriteLine($"Attempting to connect to {Address}:{Port} with current session");

            try
            {
                await heartbeat.ConnectAsync(HeartUri, new HttpMessageInvoker(handler), default);
            }
            catch (Exception e)
            {
                if (heartbeat.HttpStatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"Session is not authorized to access {Address}:{Port}, attempting password authentication...");

                    var credentials = new Dictionary<string, string>();
                    credentials.Add("username", Username);

                    var password = await _getPassword($"Authenticating to {Address}:{Port} as {Username}");
                    credentials.Add("password", password);

                    await PostForm(AuthUri, credentials);

                    heartbeat = new ClientWebSocket();

                    await heartbeat.ConnectAsync(HeartUri, new HttpMessageInvoker(handler), default);
                }
                else
                {
                    throw e;
                }
            }

            Console.WriteLine($"Connected to {Address}:{Port} as {Username}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not connect to {Address}:{Port} due to: {e}");

            //TODO: better error handling

            lock (_heartLock)
            {
                _heartBeating = false;
            }

            return;
        }

        while (!_cts.Token.IsCancellationRequested)
        {
            var bytes = new byte[17];
            await heartbeat.ReceiveAsync(bytes, default);
            var message = await FetchMessage(new Guid(bytes.AsSpan(1, 16)));
            await _onMessageReceived(this, message);
        }

        await heartbeat.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);

        lock (_heartLock)
        {
            _heartBeating = false;
        }
    }

    public async Task<Message> FetchMessage(Guid uuid)
    {
        var bytes = await _httpClient.GetByteArrayAsync(new Uri(MsgUri, uuid.ToString()));
        var obj = CBORObject.DecodeFromBytes(bytes);
        return new Message(obj);
    }

    public async Task<User> FetchUser(Guid uuid)
    {
        var bytes = await _httpClient.GetByteArrayAsync(new Uri(UsrUri, uuid.ToString()));
        var obj = CBORObject.DecodeFromBytes(bytes);
        return new User(obj);
    }

    public async Task<Thread> FetchThread(Guid uuid)
    {
        var bytes = await _httpClient.GetByteArrayAsync(new Uri(ThrUri, uuid.ToString()));
        var obj = CBORObject.DecodeFromBytes(bytes);
        return new Thread(obj);
    }
}
