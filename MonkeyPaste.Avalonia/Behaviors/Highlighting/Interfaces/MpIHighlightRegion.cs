using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHighlightRegion {
        event EventHandler<int> MatchCountChanged;
        int Priority { get; }

        MpHighlightType HighlightType { get; }

        MpContentQueryBitFlags AcceptanceFlags { get; }

        bool IsVisible { get; }

        int MatchCount { get; }
        int SelectedIdx { get; }
        //int ContentItemIdx { get; }

        Task FindHighlightingAsync();
        Task ApplyHighlightingAsync();
        void Reset();
        //Task ScrollToSelectedItemAsync();
        //void SelectNextMatch();
        //void SelectPreviousMatch();
    }
}
