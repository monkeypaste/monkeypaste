using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpHighlightSelectorBehavior : Behavior<MpAvClipTileView> {
        List<MpIHighlightRegion> _items = new List<MpIHighlightRegion>();
        List<MpIHighlightRegion> Items =>
            _items;

        MpIHighlightRegion SelectedItem =>
            _selectedHighlighterIdx >= 0 && _selectedHighlighterIdx < Items.Count ?
            Items[_selectedHighlighterIdx] :
            null;

        int SelectedMatchIdx =>
            SelectedItem == null ? -1 : SelectedItem.SelectedIdx;

        private int _selectedHighlighterIdx = 0;

        bool IsActive =>
            Items.Any(x => x.MatchCount > 0);
        protected override void OnAttached() {
            base.OnAttached();

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            //AssociatedObject.AttachedToVisualTree += AssociatedObject_AttachedToVisualTree;

        }

        //private void AssociatedObject_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
        //    throw new System.NotImplementedException();
        //}

        private void AssociatedObject_DataContextChanged(object sender, System.EventArgs e) {
            if (AssociatedObject == null ||
                AssociatedObject.DataContext == null ||
                string.IsNullOrEmpty(MpAvQueryViewModel.Instance.MatchValue)) {
                Reset();
            }
            if (!Items.Any()) {

                Items.Clear();
                if (AssociatedObject.FindControl<Control>("ClipTileContentView") is Control ctcv) {
                    if (ctcv.FindControl<ContentControl>("ClipTileContentControl") is ContentControl ctcc) {
                        // content
                        Items.Add(Interaction.GetBehaviors(ctcc).FirstOrDefault() as MpIHighlightRegion);

                    }
                }

                if (AssociatedObject.FindControl<MpAvClipTileTitleView>("TileTitleView") is MpAvClipTileTitleView cttv) {
                    if (cttv.FindControl<MpAvMarqueeTextBox>("TileTitleTextBox") is MpAvMarqueeTextBox mtb) {
                        // title
                        Items.Add(Interaction.GetBehaviors(mtb).FirstOrDefault() as MpIHighlightRegion);
                    }
                    if (cttv.FindControl<Button>("ClipTileAppIconImageButton") is Button b) {
                        // source
                        Items.Add(Interaction.GetBehaviors(b).FirstOrDefault() as MpIHighlightRegion);
                    }
                }
                _items = Items.OrderBy(x => x.Priority).ToList();
            }
            PerformHighlighting().FireAndForgetSafeAsync();
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
            Reset();
        }
        private async void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {

                case MpMessageType.SelectNextMatch:
                case MpMessageType.SelectPreviousMatch:
                    if (msg == MpMessageType.SelectNextMatch) {
                        SelectNextMatch();
                    } else {
                        SelectPreviousMatch();
                    }
                    break;
                case MpMessageType.RequeryCompleted:
                    Reset();
                    await PerformHighlighting();
                    break;
                case MpMessageType.JumpToIdxCompleted:
                    await Task.Delay(500);
                    await PerformHighlighting();
                    break;
            }
        }

        private async Task PerformHighlighting() {
            if (AssociatedObject != null &&
                AssociatedObject.BindingContext != null) {
                while (AssociatedObject.BindingContext.IsAnyBusy) {
                    await Task.Delay(100);
                }
            }
            if (string.IsNullOrEmpty(MpAvQueryViewModel.Instance.MatchValue)) {
                Reset();
                return;
            }

            await Task.WhenAll(Items.Select(x => x.FindHighlightingAsync()));
            _selectedHighlighterIdx = 0;
            if (Items.All(x => x.MatchCount == 0)) {
                return;
            }
            if (Items.Any(x => x.MatchCount > 1)) {
                MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
            SelectNextMatch();
            Items.ForEach(x => x.ApplyHighlightingAsync());
        }

        private void SelectNextItem() {
            if (!IsActive) {
                return;
            }
            SelectedItem.SelectedIdx = -1;
            do {
                _selectedHighlighterIdx++;
                if (_selectedHighlighterIdx >= Items.Count) {
                    _selectedHighlighterIdx = 0;
                }
            } while (SelectedItem.MatchCount == 0);

            SelectedItem.SelectedIdx = 0;
        }
        private void SelectPrevItem() {
            if (!IsActive) {
                return;
            }
            SelectedItem.SelectedIdx = -1;
            do {
                _selectedHighlighterIdx--;
                if (_selectedHighlighterIdx < 0) {
                    _selectedHighlighterIdx = Items.Count - 1;
                }
            } while (SelectedItem.MatchCount == 0);

            SelectedItem.SelectedIdx = SelectedItem.MatchCount - 1;
        }
        private void SelectNextMatch() {
            if (!IsActive) {
                return;
            }
            int next_idx = SelectedMatchIdx + 1;
            if (next_idx >= SelectedItem.MatchCount) {
                SelectNextItem();
            } else {
                SelectedItem.SelectedIdx = next_idx;
            }
            //if (Items[_selectedHighlighterIdx].SelectedIdx >= Items[_selectedHighlighterIdx].MatchCount - 1) {
            //    Items[_selectedHighlighterIdx].SelectedIdx = -1;
            //    _selectedHighlighterIdx++;
            //}
            //if (_selectedHighlighterIdx == Items.Count) {
            //    // _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
            //    _selectedHighlighterIdx = 0;
            //    Items[_selectedHighlighterIdx].SelectedIdx = 0;
            //} else {
            //    Items[_selectedHighlighterIdx].SelectedIdx++;
            //}


            //if (Items[_selectedHighlighterIdx].MatchCount == 0) {
            //    SelectNextMatch();
            //    return;
            //}
            Items.ForEach(x => x.ApplyHighlightingAsync());
        }

        private void SelectPreviousMatch() {
            if (!IsActive) {
                return;
            }
            int prev_idx = SelectedMatchIdx - 1;
            if (prev_idx < 0) {
                SelectPrevItem();
            } else {
                SelectedItem.SelectedIdx = prev_idx;
            }
            //if (Items.All(x => x.MatchCount == 0)) {
            //    return;
            //}
            //if (Items[_selectedHighlighterIdx].SelectedIdx == 0) {
            //    Items[_selectedHighlighterIdx].SelectedIdx = -1;
            //    if (_selectedHighlighterIdx == 0) {
            //        _selectedHighlighterIdx = Items.Count - 1;
            //    } else {
            //        _selectedHighlighterIdx--;
            //    }
            //    Items[_selectedHighlighterIdx].SelectedIdx = Items[_selectedHighlighterIdx].MatchCount - 1;
            //} else {
            //    Items[_selectedHighlighterIdx].SelectedIdx--;

            //    //NOTE this added for untested stack overflow of decrementing SelectedIdx
            //    //from here
            //    if (Items[_selectedHighlighterIdx].SelectedIdx < 0) {
            //        if (_selectedHighlighterIdx == 0) {
            //            _selectedHighlighterIdx = Items.Count - 1;
            //        } else {
            //            _selectedHighlighterIdx--;
            //        }
            //        Items[_selectedHighlighterIdx].SelectedIdx = Items[_selectedHighlighterIdx].MatchCount - 1;
            //    }
            //    //to here
            //}
            //if (_selectedHighlighterIdx >= _highlighters.Count - 1) {
            //    _highlighters[_selectedHighlighterIdx].SelectedIdx = -1;
            //    _selectedHighlighterIdx = 0;
            //}


            //if (Items[_selectedHighlighterIdx].MatchCount == 0) {
            //    SelectPreviousMatch();
            //    return;
            //}

            Items.ForEach(x => x.ApplyHighlightingAsync());
        }

        //private void Reset(List<MpIHighlightRegion> _highlighters) {
        //    _selectedHighlighterIdx = 0;
        //    _highlighters.ForEach(x => x.Reset());
        //}
        private void Reset() {
            _selectedHighlighterIdx = 0;
            Items.ForEach(x => x.Reset());
        }
    }
}
