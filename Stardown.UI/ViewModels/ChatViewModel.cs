using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stardown.Core.Data;

namespace Stardown.UI.ViewModels;

internal partial class ChatViewModel : ViewModelBase
{
    private MainViewModel _parent;

    [ObservableProperty]
    private string _editText = string.Empty;

    public ChatViewModel(MainViewModel parent)
    {
        _parent = parent;
    }

    // void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    // {
    //     // This event is raised from another thread so we have to transfer to UI thread.
    //     // There might be a cleaner ReactiveUI way of doing this.
    //     Dispatcher.UIThread.Invoke(() =>
    //     {
    //         this.Messages.Add(new ChatMessageViewModel { Text = args.Message.Text });
    //     });
    // }

    public ObservableCollection<MessageViewModel> Messages { get; } = [];

    [RelayCommand]
    private void SendMessage()
    {
        if (_parent.Server is Server server && _parent.Thread is Thread thread)
        {
            server.SendMessage(thread.Uuid, this.EditText, null);
            this.EditText = string.Empty;
        }
    }
}
