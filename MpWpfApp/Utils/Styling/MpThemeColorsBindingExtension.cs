using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MpWpfApp {
    //https://thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
    public class MpThemeColorsBindingExtension : Binding {
        public MpThemeColorsBindingExtension() {
            Initialize();
        }

        public MpThemeColorsBindingExtension(string path) : base(path) {
            Initialize();
        }

        private void Initialize() {
            this.Source = MpThemeColors.Instance;
            this.Mode = BindingMode.OneWay;
        }
    }
}
