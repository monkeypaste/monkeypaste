using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHighlightRegion {
        int Priority { get; }

        MpHighlightType HighlightType { get; }

        bool IsVisible { get; }

        int MatchCount { get; }
        int SelectedIdx { get; set; }
        int ContentItemIdx { get; }

        Task FindHighlightingAsync();
        Task ApplyHighlightingAsync();
        void Reset();
        Task ScrollToSelectedItemAsync();
        //void SelectNextMatch();
        //void SelectPreviousMatch();
    }
}
