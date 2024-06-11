using MonkeyPaste;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public interface MpIHighlightTextRangesInfoViewModel : MpIViewModel {
        ObservableCollection<MpTextRange> HighlightRanges { get; set; }
        int ActiveHighlightIdx { get; set; }
    }
}

