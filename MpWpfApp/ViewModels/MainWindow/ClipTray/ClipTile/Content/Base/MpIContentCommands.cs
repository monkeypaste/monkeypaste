using System.Windows.Input;

namespace MpWpfApp {
    public interface MpIContentCommands {
        //ChangeColor - Brush
        //HotkeyPaste = object
        //Translate string
        ICommand AssignHotkeyCommand { get; }
        ICommand BringToFrontCommand { get; }
        ICommand ChangeColorCommand { get; }
        ICommand CopyCommand { get; }
        ICommand CreateQrCodeCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand DuplicateCommand { get; }
        ICommand EditContentCommand { get; }
        ICommand EditTitleCommand { get; } 
        ICommand ExcludeApplicationCommand { get; }
        ICommand HotkeyPasteCommand { get; }
        ICommand InvertSelectionCommand { get; }
        ICommand LinkTagToContentCommand { get; }
        ICommand LoadMoreClipsCommand { get; }
        ICommand MergeCommand { get; }
        ICommand PasteCommand { get; }
        ICommand SearchWebCommand { get; }
        ICommand SelectAllCommand { get; }
        ICommand SelectNextCommand { get; }
        ICommand SelectPreviousCommand { get; }
        ICommand SendToEmailCommand { get; }
        ICommand SendToBackCommand { get; }
        ICommand SpeakCommand { get; }
        ICommand TranslateCommand { get; }
    }
}
