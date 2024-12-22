using Avalonia.Controls;
using Avalonia.Input;
using Stardown.UI.ViewModels;

namespace Stardown.UI.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();

        // CommunicationService.Instance.MessageReceived += OnMessageReceived;
    }

    public void OnEditKeyUp(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            if (this.DataContext is ChatViewModel chatViewModel)
            {
                chatViewModel.SendCommand.Execute(null);
            }
        }
    }

    // void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    // {
    //     Dispatcher.UIThread.Invoke(() => {
    //         scrollViewer.ScrollToEnd();
    //     });
    // }
}
