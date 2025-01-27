using System.Net;
using System.Net.WebSockets;
using FluentUri;
using PeterO.Cbor;

namespace Stardown.Core.Data; //TODO: System.Uri is horrible, implement a custom Uri class

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

    public FluentUriBuilder BaseUri
    {
        get
        {
            return FluentUriBuilder.Create()
                .Scheme(UriScheme.Https)
                .Host(Address)
                .Port(Port);
        }
    }

    public Uri ThrUri
    {
        get
        {
            return BaseUri
                .Path("thr")
                .ToUri();
        }
    }

    public Uri UsrUri
    {
        get
        {
            return BaseUri
                .Path("usr")
                .ToUri();
        }
    }

    public Uri MsgUri
    {
        get
        {
            return BaseUri
                .Path("msg")
                .ToUri();
        }
    }

    public Uri AuthUri
    {
        get
        {
            return BaseUri
                .Path("auth")
                .ToUri();
        }
    }

    public Uri HeartUri
    {
        get
        {
            return new Uri($"wss://{Address}:{Port}/heart", UriKind.Absolute);
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

            Console.WriteLine("Ba-bump");

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
        var uri = new Uri($"{MsgUri}/{uuid}");
        Console.WriteLine($"Fetching message from {uri}");

        var bytes = await _httpClient.GetByteArrayAsync(uri);
        var obj = CBORObject.DecodeFromBytes(bytes);

        return new Message(obj);
    }

    public async Task<User> FetchUser(Guid uuid)
    {
        var uri = new Uri($"{UsrUri}/{uuid}");
        Console.WriteLine($"Fetching user from {uri}");

        var bytes = await _httpClient.GetByteArrayAsync(uri);
        var obj = CBORObject.DecodeFromBytes(bytes);

        return new User(obj);
    }

    public async Task<Thread> FetchThread(Guid uuid)
    {
        var uri = new Uri($"{ThrUri}/{uuid}");
        Console.WriteLine($"Fetching thread from {uri}");

        var bytes = await _httpClient.GetByteArrayAsync(uri);
        var obj = CBORObject.DecodeFromBytes(bytes);

        return new Thread(obj);
    }
}
