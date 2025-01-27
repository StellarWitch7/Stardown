using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DesktopNotifications;
using Stardown.Core.Data;

namespace Stardown.UI.ViewModels;

internal partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<Server> Servers { get; set; } = new ObservableCollection<Server>();
    public ObservableCollection<Thread> Threads { get; set; } = new ObservableCollection<Thread>();

    [ObservableProperty]
    private ChatViewModel _chat;
    [ObservableProperty]
    private Server? _server;
    [ObservableProperty]
    private Thread? _thread;

    public MainViewModel()
    {
        _chat = new ChatViewModel(this);
        Servers.Add(new Server("test", "localhost", 63063)); //TODO: testing
    }


    partial void OnServerChanging(Server? value)
    {
        value?.ConnectHeartbeat(GetPassword, OnMessageReceived);
    }

    async Task<string> GetPassword(string purpose)
    {
        return "test"; //TODO: uh thing (https://docs.avaloniaui.net/docs/tutorials/music-store-app/opening-a-dialog)
    }

    async Task OnMessageReceived(Server server, Message message)
    {
        var sender = await server.FetchUser(message.SenderUuid);
        var thread = await server.FetchThread(message.ThreadUuid);
        var notification = new Notification()
        {
            Title = $"#{thread.Name}",
            Body = $"{sender.Name}: {message.Contents}"
        };

        await Notifier.Manager.ShowNotification(notification);
    }
}
