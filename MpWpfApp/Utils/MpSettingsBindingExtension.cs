using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpSettingBindingExtension : Binding {
        public MpSettingBindingExtension() {
            Initialize();
        }

        public MpSettingBindingExtension(string path)
            : base(path) {
            Initialize();
        }

        private void Initialize() {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
