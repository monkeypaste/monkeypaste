using MonkeyPaste;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;

namespace MonkeyPaste;
public interface MpIHighlightTextRangesInfoViewModel : MpIViewModel {
    ObservableCollection<MpTextRange> HighlightRanges { get; set; }
    int ActiveHighlightIdx { get; set; }
}
