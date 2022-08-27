using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIHighlightRegion {
        int Priority { get; }

        MpHighlightType HighlightType { get; }

        bool IsVisible { get; }

        int MatchCount { get; }
        int SelectedIdx { get; set; }
        int ContentItemIdx { get; }

        Task FindHighlighting();
        void ApplyHighlighting();
        void Reset();
        void ScrollToSelectedItem();
        //void SelectNextMatch();
        //void SelectPreviousMatch();
    }
}
