
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollectionBase : MpIPlatformScreenInfoCollection {
        private MpAvScreenInfoComparer _comparer = new MpAvScreenInfoComparer();
        public ObservableCollection<MpIPlatformScreenInfo> Screens { get; protected set; }

        public MpIPlatformScreenInfo Primary {
            get {
                if (Screens == null) {
                    return null;
                }
                if (Screens.FirstOrDefault(x => x.IsPrimary) is { } ps) {
                    return ps;
                }
                return Screens.FirstOrDefault();
            }
        }

        public MpAvScreenInfoCollectionBase(IEnumerable<MpIPlatformScreenInfo> sil) {
            Screens = new ObservableCollection<MpIPlatformScreenInfo>(sil);
        }
        public virtual bool Refresh() {
            // returns true when info changes

            // NOTE this should be called before referencing screens
            if (MpAvWindowManager.Screens is not { } scrs) {
                return false;
            }
            var cur_screens = scrs.All.Select(x => new MpAvDesktopScreenInfo(x));
            var diffs = Screens.Difference(cur_screens, _comparer).ToList();
            if (diffs.Any()) {
                MpConsole.WriteLine($"Screen info changed. Diffs:", true);
                diffs.ForEach((x, idx) => MpConsole.WriteLine($"[{(Screens.Contains(x) ? "OLD" : "NEW")}] {x}", false, idx == diffs.Count - 1));
                Screens.Clear();
                Screens.AddRange(cur_screens);
                MpMessenger.SendGlobal(MpMessageType.ScreenInfoChanged);
                return true;
            }
            return false;
        }
    }
    public class MpAvScreenInfoComparer : IEqualityComparer<MpIPlatformScreenInfo> {
        public bool Equals(MpIPlatformScreenInfo x, MpIPlatformScreenInfo y) {
            return
                x.IsEqual(y);
        }

        public int GetHashCode([DisallowNull] MpIPlatformScreenInfo obj) {
            return obj.GetHashCode();
        }
    }
}
