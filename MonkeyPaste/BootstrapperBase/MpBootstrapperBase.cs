using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        protected static MpLoaderBalloonViewModel _loader;
        
        protected static List<MpBootstrappedItem> _items = new List<MpBootstrappedItem>();

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw, MpLoaderBalloonViewModel loader) {
            _loader = loader;

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    new MpBootstrappedItem(typeof(MpConsole)),
                    new MpBootstrappedItem(typeof(MpNativeWrapper),niw),
                    new MpBootstrappedItem(typeof(MpPreferences),niw.GetPreferenceIO()),                    
                    new MpBootstrappedItem(typeof(MpRegEx)),
                    new MpBootstrappedItem(typeof(MpDb),niw.GetDbInfo()),
                    new MpBootstrappedItem(typeof(MpDataModelProvider),niw.GetQueryInfo()),
                    new MpBootstrappedItem(typeof(MpPluginManager))
                }
                );
        }

        public abstract Task Initialize();

        protected void ReportItemLoading(MpBootstrappedItem item, int index) {
            MpConsole.WriteLine("Loading " + item.Label + " at idx: " + index);
            _loader.Info = $"Loading {item.Label}";
            _loader.PercentLoaded = (double)((double)(index + 1) / (double)_items.Count);
        }
    }
}



            