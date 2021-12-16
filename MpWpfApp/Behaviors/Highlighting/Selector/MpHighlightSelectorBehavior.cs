using MonkeyPaste;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpHighlightSelectorBehavior : MpBehavior<MpClipTileView> {
        private List<MpIHighlightRegion> _highlighters {
            get {
                List<MpIHighlightRegion> dtl = new List<MpIHighlightRegion>();

                var rtbvl = AssociatedObject.GetVisualDescendents<MpRtbView>();
                dtl.AddRange(rtbvl.Select(x => x.RtbHighlightBehavior).ToList());

                var flivl = AssociatedObject.GetVisualDescendents<MpFileListItemView>();
                dtl.AddRange(flivl.Select(x => x.FileListItemHighlightBehavior).ToList());

                dtl.Add(AssociatedObject.TileTitleView.ClipTileTitleHighlightBehavior);
                dtl.Add(AssociatedObject.TileTitleView.SourceHighlightBehavior);

                dtl.Sort((x, y) => x.ContentItemIdx.CompareTo(y.ContentItemIdx));
                return dtl;
            }
        }

        private int _selectedHighlighterIdx = 0;

        protected override void OnLoad() {
            if(_isLoaded) {
                return;
            }
            base.OnLoad();
            
            Reset();

            MpMessenger.Instance.Register<MpMessageType>(
                    MpSearchBoxViewModel.Instance,
                    ReceivedSearchBoxViewModelMessage);

            MpMessenger.Instance.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);
        }

        protected override void OnUnload() {
            base.OnUnload();

            MpMessenger.Instance.Unregister<MpMessageType>(
                    MpSearchBoxViewModel.Instance,
                    ReceivedSearchBoxViewModelMessage);

            MpMessenger.Instance.Unregister<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

            Reset();
        }

        private void ReceivedSearchBoxViewModelMessage(MpMessageType msg) {
            if (_highlighters.All(x => x.MatchCount == 0)) {
                return;
            }
            switch (msg) {
                case MpMessageType.SelectNextMatch:
                    SelectNextMatch();
                    break;
                case MpMessageType.SelectPreviousMatch:
                    SelectPreviousMatch();
                    break;
            }
        }

        private async void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    if (_highlighters.Count == 0 ||
                        string.IsNullOrEmpty(MpDataModelProvider.Instance.QueryInfo.SearchText)) {
                        Reset();
                        break;
                    }

                    await Task.WhenAll(_highlighters.Select(x => x.FindHighlighting()));
                    _selectedHighlighterIdx = 0;
                    if(_highlighters.All(x=>x.MatchCount == 0)) {
                        return;
                    }
                    SelectNextMatch();
                    _highlighters.ForEach(x => x.ApplyHighlighting());
                    break;
            }
        }

        public void SelectNextMatch() {
            if(_highlighters[_selectedHighlighterIdx].SelectedIdx >= _highlighters[_selectedHighlighterIdx].MatchCount - 1) {
                _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
                _selectedHighlighterIdx++;
            }
            if(_selectedHighlighterIdx == _highlighters.Count) {
               // _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
                _selectedHighlighterIdx = 0;
                _highlighters[_selectedHighlighterIdx].SelectedIdx = 0;
            } else {
                _highlighters[_selectedHighlighterIdx].SelectedIdx++;
            }
            

            if(_highlighters[_selectedHighlighterIdx].MatchCount == 0) {
                SelectNextMatch();
                return;
            }
            _highlighters.ForEach(x => x.ApplyHighlighting());
        }

        public void SelectPreviousMatch() {
            if (_highlighters[_selectedHighlighterIdx].SelectedIdx == 0){
                _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
                if (_selectedHighlighterIdx == 0) {
                    _selectedHighlighterIdx = _highlighters.Count - 1;
                } else {
                    _selectedHighlighterIdx--;
                }
                _highlighters[_selectedHighlighterIdx].SelectedIdx = _highlighters[_selectedHighlighterIdx].MatchCount - 1;
            } else {
                _highlighters[_selectedHighlighterIdx].SelectedIdx--;
            }
            //if (_selectedHighlighterIdx >= _highlighters.Count - 1) {
            //    _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
            //    _selectedHighlighterIdx = 0;
            //}
            

            if (_highlighters[_selectedHighlighterIdx].MatchCount == 0) {
                SelectPreviousMatch();
                return;
            }

            _highlighters.ForEach(x => x.ApplyHighlighting());
        }

        private void Reset() {
            _selectedHighlighterIdx = 0;
            _highlighters.ForEach(x => x.Reset());
        }
    }
}
