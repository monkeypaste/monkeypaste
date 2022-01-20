using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpBootstrapperBase {
        private List<MpBootstrappedItem> _items;

        public MpBootstrapperBase(MpINativeInterfaceWrapper niw) {
            _items = new List<MpBootstrappedItem>() {
                new MpBootstrappedItem(typeof(MpRegEx)),
                new MpBootstrappedItem(typeof(MpHelpers)),
                new MpBootstrappedItem(typeof(MpMessenger)),
                new MpBootstrappedItem(typeof(MpNativeWrapper),"niw",niw),
                new MpBootstrappedItem(typeof(MpPreferences),"prefIo",niw.GetPreferenceIO()),
                new MpBootstrappedItem(typeof(MpDb),"dbInfo",niw.GetDbInfo()),
                new MpBootstrappedItem(typeof(MpDataModelProvider),"queryInfo",niw.GetQueryInfo()),
                new MpBootstrappedItem(typeof(MpPluginManager)),
            };
        }

        public virtual async Task Initialize() {
            for (int i = 0; i < _items.Count; i++) {
                await _items[i].Register();
            }            
        }
    }
}



            