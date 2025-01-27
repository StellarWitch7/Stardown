using Avalonia.Controls;
using Avalonia.Input;
using Stardown.UI.ViewModels;

namespace Stardown.UI.Views;

public partial class ChatView : UserControl
{
    public ChatView()
    {
        InitializeComponent();
    }

    public void OnEditKeyUp(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            if (this.DataContext is ChatViewModel chatViewModel)
            {
                chatViewModel.SendMessageCommand.Execute(null);
            }
        }
    }
}
