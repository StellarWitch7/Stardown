using Avalonia.Threading;
using ReactiveUI;
using Stardown.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Stardown.Core.ViewModels
{
    internal class ChatViewModel : ViewModelBase
    {
        private string _editText = string.Empty;

        public ChatViewModel()
        {
            this.SendCommand = ReactiveCommand.Create(ExecuteSend);

            CommunicationService.Instance.MessageReceived += OnMessageReceived;
        }

        void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            // This event is raised from another thread so we have to transfer to UI thread.
            // There might be a cleaner ReactiveUI way of doing this.
            Dispatcher.UIThread.Invoke(() =>
            {
                this.Messages.Add(new ChatMessageViewModel { Text = args.Message.Text });
            });
        }

        public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

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

        public ICommand SendCommand { get; }

        private void ExecuteSend()
        {
            var message = new Message { Text = this._editText };

            // Sending needs to be done without blocking
            CommunicationService.Instance.SendMessage(message);

            this.EditText = string.Empty;
        }
    }
}
