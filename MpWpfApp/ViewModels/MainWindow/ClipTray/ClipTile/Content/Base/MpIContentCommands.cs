using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public interface MpIContentCommands {
        ICommand AssignHotkeyCommand { get; }
        IAsyncCommand BringToFrontAsyncCommand { get; }
        IAsyncCommand<Brush> ChangeColorAsyncCommand { get; }
        IAsyncCommand CopyAsyncCommand { get; }
        ICommand CreateQrCodeCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand DuplicateCommand { get; }
        ICommand EditContentCommand { get; }
        ICommand EditTitleCommand { get; }
        ICommand ExcludeApplicationCommand { get; }
        IAsyncCommand<object> HotkeyPasteAsyncCommand { get; }
        ICommand InvertSelectionCommand { get; }
        ICommand LinkTagToContentCommand { get; }
        ICommand LoadMoreClipsCommand { get; }
        IAsyncCommand MergeAsyncCommand { get; }
        IAsyncCommand PasteAsyncCommand { get; }
        ICommand SearchWebCommand { get; }
        ICommand SelectAllCommand { get; }
        ICommand SelectNextCommand { get; }
        ICommand SelectPreviousCommand { get; }
        ICommand SendToEmailCommand { get; }
        IAsyncCommand SendToBackAsyncCommand { get; }
        IAsyncCommand SpeakAsyncCommand { get; }
        IAsyncCommand<string> TranslateAsyncCommand { get; }
    }
}
