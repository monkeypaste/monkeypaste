using Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollectionBase : MpIPlatformScreenInfoCollection {
        private MpAvScreenInfoComparer _comparer = new MpAvScreenInfoComparer();
        public ObservableCollection<MpIPlatformScreenInfo> Screens { get; protected set; } = [];
        public IEnumerable<MpIPlatformScreenInfo> All =>
            Screens;
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

        public MpAvScreenInfoCollectionBase(MpAvWindow w) {
            if (w is null ||
                w.Screens is not { } scrns) {
                return;
            }
            scrns.All.Select(x => new MpAvDesktopScreenInfo(x));
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
            IList<MpIPlatformScreenInfo> cur_screens = scrs.All.Select(x => new MpAvDesktopScreenInfo(x)).ToList<MpIPlatformScreenInfo>();
            //var diffs = Screens.Difference(cur_screens, _comparer);
            bool has_changed = HasScreensChanged(Screens, cur_screens);
            if (has_changed) {
                MpConsole.WriteLine($"Screen info changed.", true);
                MpConsole.WriteLine($"Old:");
                Screens.ForEach(x => MpConsole.WriteLine(x.ToString()));
                MpConsole.WriteLine($"New:");
                cur_screens.ForEach(x => MpConsole.WriteLine(x.ToString()));
                MpConsole.WriteLine($"");

                Screens.Clear();
                Screens.AddRange(cur_screens);
                MpMessenger.SendGlobal(MpMessageType.ScreenInfoChanged);
                return true;
            }
            return false;
        }

        private bool HasScreensChanged(IList<MpIPlatformScreenInfo> a, IList<MpIPlatformScreenInfo> b) {
            if(a == b && b == null) {
                return false;
            }
            if (((a == null || b == null) && a != b) ||
                a.Count != b.Count) {
                return true;
            }
            foreach (var a_s in a) {
                bool has_match = false;
                foreach (var b_s in b) {
                    if (a_s.ToString() == b_s.ToString()) {
                        has_match = true;
                        break;
                    }
                }
                if (!has_match) {
                    return true;
                }
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
