using CommunityToolkit.Mvvm.ComponentModel;

namespace Stardown.UI.ViewModels;

internal partial class MessageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _contents = string.Empty;
}
