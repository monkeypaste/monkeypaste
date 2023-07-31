using MonkeyPaste;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;

namespace MonkeyPaste;
public interface MpIHighlightTextRangesInfoViewModel : MpIViewModel {
    ObservableCollection<MpTextRange> HighlightRanges { get; }
    int ActiveHighlightIdx { get; set; }
}
