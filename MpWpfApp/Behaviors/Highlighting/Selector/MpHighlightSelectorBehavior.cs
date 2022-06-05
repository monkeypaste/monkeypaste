using MonkeyPaste;
using MonkeyPaste.Common.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpHighlightSelectorBehavior : MpBehavior<MpClipTileView> {
        //private List<MpIHighlightRegion> _highlighters {
        //    get {
        //        List<MpIHighlightRegion> dtl = new List<MpIHighlightRegion>();
        //        if(AssociatedObject == null) {
        //            return dtl;
        //        }

        //        var rtbvl = AssociatedObject.GetVisualDescendents<MpRtbView>();
        //        dtl.AddRange(rtbvl.Select(x => x.RtbHighlightBehavior).ToList());

        //        var flivl = AssociatedObject.GetVisualDescendents<MpFileListItemView>();
        //        dtl.AddRange(flivl.Select(x => x.FileListItemHighlightBehavior).ToList());

        //        dtl.Add(AssociatedObject.TileTitleView.ClipTileTitleHighlightBehavior);
        //        dtl.Add(AssociatedObject.TileTitleView.SourceHighlightBehavior);

        //        dtl.Sort((x, y) => x.ContentItemIdx.CompareTo(y.ContentItemIdx));
        //        return dtl;
        //    }
        //}

        private int _selectedHighlighterIdx = 0;

        protected override async void OnLoad() {
            if(_isLoaded) {
                return;
            }
            base.OnLoad();
            
            MpMessenger.Register<MpMessageType>(
                    MpSearchBoxViewModel.Instance,
                    ReceivedSearchBoxViewModelMessage);

            MpMessenger.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

            if(!string.IsNullOrEmpty(MpDataModelProvider.QueryInfo.SearchText)) {
                while (AssociatedObject == null) {
                    await Task.Delay(10);
                }
                await PerformHighlighting();
            } else {
                var hll = await GetHighlighters();
                Reset(hll);
            }
        }

        protected override async void OnUnload() {
            base.OnUnload();

            MpMessenger.Unregister<MpMessageType>(
                    MpSearchBoxViewModel.Instance,
                    ReceivedSearchBoxViewModelMessage);

            MpMessenger.Unregister<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);

            var hll = await GetHighlighters();
            Reset(hll);
        }

        private async Task<List<MpIHighlightRegion>> GetHighlighters() {
            List<MpIHighlightRegion> dtl = new List<MpIHighlightRegion>();
            while (AssociatedObject == null) {
                await Task.Delay(50);
            }

            var rtbvl = AssociatedObject.GetVisualDescendents<MpContentView>();
            dtl.AddRange(rtbvl.Select(x => x.RtbHighlightBehavior).ToList());

            dtl.Add(AssociatedObject.TileTitleView.ClipTileTitleHighlightBehavior);
            dtl.Add(AssociatedObject.TileTitleView.SourceHighlightBehavior);

            dtl.Sort((x, y) => x.ContentItemIdx.CompareTo(y.ContentItemIdx));
            return dtl;
        }

        private async void ReceivedSearchBoxViewModelMessage(MpMessageType msg) {            
            switch (msg) {
                case MpMessageType.SelectNextMatch:
                case MpMessageType.SelectPreviousMatch:
                    var hll = await GetHighlighters();
                    if(msg == MpMessageType.SelectNextMatch) {
                        SelectNextMatch(hll);
                    } else {
                        SelectPreviousMatch(hll);
                    }
                    break;
            }
        }

        private async void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    await PerformHighlighting();
                    break;
                case MpMessageType.JumpToIdxCompleted:
                    await Task.Delay(500);
                    await PerformHighlighting();
                    break;
            }
        }

        private async Task PerformHighlighting() {
            var hll = await GetHighlighters();

            if (hll.Count == 0 ||
                        string.IsNullOrEmpty(MpDataModelProvider.QueryInfo.SearchText)) {
                Reset(hll);
                return;
            }

            await Task.WhenAll(hll.Select(x => x.FindHighlighting()));
            _selectedHighlighterIdx = 0;
            if (hll.All(x => x.MatchCount == 0)) {
                return;
            }
            SelectNextMatch(hll);
            hll.ForEach(x => x.ApplyHighlighting());
        }
        public void SelectNextMatch(List<MpIHighlightRegion> hll) {
            if (hll.All(x => x.MatchCount == 0)) {
                return;
            }
            if (hll[_selectedHighlighterIdx].SelectedIdx >= hll[_selectedHighlighterIdx].MatchCount - 1) {
                hll[_selectedHighlighterIdx].SelectedIdx = -1;
                _selectedHighlighterIdx++;
            }
            if(_selectedHighlighterIdx == hll.Count) {
               // _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
                _selectedHighlighterIdx = 0;
                hll[_selectedHighlighterIdx].SelectedIdx = 0;
            } else {
                hll[_selectedHighlighterIdx].SelectedIdx++;
            }
            

            if(hll[_selectedHighlighterIdx].MatchCount == 0) {
                SelectNextMatch(hll);
                return;
            }
            hll.ForEach(x => x.ApplyHighlighting());
        }

        public void SelectPreviousMatch(List<MpIHighlightRegion> hll) {
            if (hll.All(x => x.MatchCount == 0)) {
                return;
            }
            if (hll[_selectedHighlighterIdx].SelectedIdx == 0){
                hll[_selectedHighlighterIdx].SelectedIdx = -1;
                if (_selectedHighlighterIdx == 0) {
                    _selectedHighlighterIdx = hll.Count - 1;
                } else {
                    _selectedHighlighterIdx--;
                }
                hll[_selectedHighlighterIdx].SelectedIdx = hll[_selectedHighlighterIdx].MatchCount - 1;
            } else {
                hll[_selectedHighlighterIdx].SelectedIdx--;
                
                //NOTE this added for untested stack overflow of decrementing SelectedIdx
                //from here
                if(hll[_selectedHighlighterIdx].SelectedIdx < 0) {
                    if (_selectedHighlighterIdx == 0) {
                        _selectedHighlighterIdx = hll.Count - 1;
                    } else {
                        _selectedHighlighterIdx--;
                    }
                    hll[_selectedHighlighterIdx].SelectedIdx = hll[_selectedHighlighterIdx].MatchCount - 1;
                }
                //to here
            }
            //if (_selectedHighlighterIdx >= _highlighters.Count - 1) {
            //    _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
            //    _selectedHighlighterIdx = 0;
            //}
            

            if (hll[_selectedHighlighterIdx].MatchCount == 0) {
                SelectPreviousMatch(hll);
                return;
            }

            hll.ForEach(x => x.ApplyHighlighting());
        }

        private void Reset(List<MpIHighlightRegion> hll) {
            _selectedHighlighterIdx = 0;
            hll.ForEach(x => x.Reset());
        }
    }
}
