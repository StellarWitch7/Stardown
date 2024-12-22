using ReactiveUI;

namespace Stardown.UI.ViewModels
{
    internal class ChatMessageViewModel : ViewModelBase
    {
        private string _text = string.Empty;

        public required string Text
        {
            get
            {
                return _text;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _text, value);
            }
        }

    }
}
