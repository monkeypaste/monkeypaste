using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHighlightRegion {
        event EventHandler<int> MatchCountChanged;
        event EventHandler<int> SelIdxChanged;
        int Priority { get; }

        MpHighlightType HighlightType { get; }

        MpContentQueryBitFlags AcceptanceFlags { get; }

        bool IsVisible { get; }

        int MatchCount { get; }
        int SelectedIdx { get; set; }
        //int ContentItemIdx { get; }

        Task FindHighlightingAsync();
        Task ApplyHighlightingAsync();
        void Reset();
        //Task ScrollToSelectedItemAsync();
        //void SelectNextMatch();
        //void SelectPreviousMatch();
    }
}
