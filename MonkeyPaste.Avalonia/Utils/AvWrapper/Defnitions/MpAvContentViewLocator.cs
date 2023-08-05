using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentViewLocator : MpIContentViewLocator {
        private List<MpIContentView> _allItems = new();
        public MpIContentView LocateContentView(int ciid) {
            if (ciid < 1) {
                return null;
            }
            var result = _allItems
                .Where(x => x.LocationId == ciid)
                .OrderByDescending(x => x.LocatedDateTime)
                .ToList();

            if (result.Count == 0) {
                // HACK _allItems not accurate sometimes, fallback to walk tree to find
                if (MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                    return
                        MpAvWindowManager.AllWindows
                            .SelectMany(x => x.GetVisualDescendants<MpAvContentWebView>())
                            .FirstOrDefault(x => x.BindingContext != null && x.BindingContext.CopyItemId == ciid && !x.BindingContext.IsPinPlaceholder);
                }
                return
                        MpAvWindowManager.AllWindows
                            .SelectMany(x => x.GetVisualDescendants<MpAvContentTextBox>())
                            .FirstOrDefault(x => x.BindingContext != null && x.BindingContext.CopyItemId == ciid && !x.BindingContext.IsPinPlaceholder);
            }
            if (result.Count > 1) {
                // is this during a pin toggle? was this item pinned?
                //MpDebug.Break();
                // remove old refs
                var stale_wvl = result.Skip(1);
                // TODO? do these need further processing? besides hiding from locator?
                _allItems = _allItems.Where(x => stale_wvl.Contains(x)).ToList();

                MpConsole.WriteLine($"{stale_wvl.Count()} stale webviews removed for item '{result[0].DataContext}'");
            }
            return result[0];
        }

        public void AddView(MpIContentView cv) {
            if (_allItems.Contains(cv)) {
                return;
            }
            // set locatedDateTime to filter out cv's recyling during
            // pin/unpin ops (especially unpinall)
            cv.LocatedDateTime = DateTime.Now;
            _allItems.Add(cv);
        }

        public void RemoveView(MpIContentView cv) {
            cv.LocatedDateTime = null;
            _allItems.Remove(cv);
        }
    }
}
