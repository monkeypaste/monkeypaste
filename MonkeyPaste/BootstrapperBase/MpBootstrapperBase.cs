using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        protected static List<MpBootstrappedItem> _items = new List<MpBootstrappedItem>();

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw) {
            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpConsole)),
                    new MpBootstrappedItem(typeof(MpNativeWrapper),niw),
                    new MpBootstrappedItem(typeof(MpCursor),niw.Cursor),
                    new MpBootstrappedItem(typeof(MpPreferences),niw.PreferenceIO),                    
                    new MpBootstrappedItem(typeof(MpRegEx)),
                    new MpBootstrappedItem(typeof(MpDb),niw.DbInfo),
                    new MpBootstrappedItem(typeof(MpDataModelProvider),niw.QueryInfo),
                    new MpBootstrappedItem(typeof(MpPluginManager))
                }
                );
        }

        public abstract Task Initialize();

        protected void ReportItemLoading(MpBootstrappedItem item, int index) {
            MpConsole.WriteLine("Loading " + item.Label + " at idx: " + index);
            MpNotificationBalloonViewModel.Instance.Info = $"Loading {item.Label}";
            MpNotificationBalloonViewModel.Instance.PercentLoaded = (double)((double)(index + 1) / (double)_items.Count);
        }
    }
}



            