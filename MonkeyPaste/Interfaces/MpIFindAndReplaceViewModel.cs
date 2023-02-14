using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpIFindAndReplaceViewModel : MpIViewModel {
        bool IsFindAndReplaceVisible { get; set; }

        string FindText { get; set; }
        string ReplaceText { get; set; }

        string FindPlaceholderText { get; }
        string ReplacePlaceholderText { get; }

        bool IsFindTextBoxFocused { get; set; }
        bool IsReplaceTextBoxFocused { get; set; }

        bool IsReplaceMode { get; set; }

        bool IsFindValid { get; }

        bool HasMatch { get; }
        bool MatchCase { get; set; }
        bool MatchWholeWord { get; set; }
        bool UseRegEx { get; set; }

        ObservableCollection<string> RecentFindTexts { get; set; }
        ObservableCollection<string> RecentReplaceTexts { get; set; }

        ICommand ToggleFindAndReplaceVisibleCommand { get; }
        ICommand FindNextCommand { get; }
        ICommand FindPreviousCommand { get; }
        ICommand ReplaceNextCommand { get; }
        ICommand ReplacePreviousCommand { get; }
        ICommand ReplaceAllCommand { get; }
    }
}
