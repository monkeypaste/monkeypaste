using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    //https://thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
    public class MpSettingBindingExtension : Binding {
        public MpSettingBindingExtension() {
            Initialize();
        }

        public MpSettingBindingExtension(string path) : base(path) {
            Initialize();
        }

        private void Initialize() {
            this.Source = MonkeyPaste.MpPreferences.Instance;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
