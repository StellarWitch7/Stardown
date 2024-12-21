using ReactiveUI;

namespace Stardown.Core.ViewModels;

internal class MainViewModel : ViewModelBase
{

    private ChatViewModel _chat;

    public MainViewModel()
    {
        _chat = new ChatViewModel();
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
}
