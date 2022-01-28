using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        private List<MpBootstrappedItem> _items = new List<MpBootstrappedItem>();

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw) {
            MpNativeWrapper.Init(niw);
            MpPreferences.Init(niw.GetPreferenceIO());
            MpRegEx.Init();
            MpDb.Init(niw.GetDbInfo());
            MpDataModelProvider.Init(niw.GetQueryInfo());
            //warning! plugin manager has issue trying to load netstandard2.1 and wpf can't load it
            MpPluginManager.Init();
        }

        public virtual async Task Initialize() {
            for (int i = 0; i < _items.Count; i++) {
                if(_items[i].ItemType == typeof(MpDb)) {
                    Debugger.Break();
                }
                await _items[i].Register();
            }            
        }
    }
}



            