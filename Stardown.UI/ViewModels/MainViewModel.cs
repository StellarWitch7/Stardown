using System.Collections.ObjectModel;
using ReactiveUI;
using Stardown.Core.Data;

namespace Stardown.UI.ViewModels;

internal class MainViewModel : ViewModelBase
{
    public ObservableCollection<Server> Servers { get; set; } = new ObservableCollection<Server>();
    public ObservableCollection<Thread> Threads { get; set; } = new ObservableCollection<Thread>();

    private ChatViewModel _chat;
    private Server? _server;
    private Thread? _thread;

    public MainViewModel()
    {
        _chat = new ChatViewModel(this);
        Servers.Add(new Server("localhost", 63063)); //TODO: testing
    }

    public ChatViewModel Chat
    {
        get
        {
            return _chat;
        }

        set
        {
            this.RaiseAndSetIfChanged(ref _chat, value);
        }
    }

    public Server? Server
    {
        get
        {
            return _server;
        }

        set
        {
            value?.ConnectHeartbeat();
            //TODO: make the server set the thread list
            this.RaiseAndSetIfChanged(ref _server, value);
        }
    }

    public Thread? Thread
    {
        get
        {
            return _thread;
        }

        set
        {
            this.RaiseAndSetIfChanged(ref _thread, value);
        }
    }
}
