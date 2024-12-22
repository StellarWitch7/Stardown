using ReactiveUI;
using Stardown.Core.Data;
using System.Windows.Input;

namespace Stardown.UI.ViewModels
{
    internal class ChatViewModel : ViewModelBase
    {
        public ICommand SendCommand { get; }

        private MainViewModel _parent;
        private string _editText = string.Empty;

        public ChatViewModel(MainViewModel parent)
        {
            _parent = parent;

            this.SendCommand = ReactiveCommand.Create(ExecuteSend);
            // CommunicationService.Instance.MessageReceived += OnMessageReceived;
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

        // public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

        public string EditText
        {
            get
            {
                return this._editText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this._editText, value);
            }
        }

        private void ExecuteSend()
        {
            if (_parent.Server is Server server && _parent.Thread is Thread thread)
            {
                server.SendMessage(thread.Uuid, EditText, null);
                this.EditText = string.Empty;
            }
        }
    }
}
