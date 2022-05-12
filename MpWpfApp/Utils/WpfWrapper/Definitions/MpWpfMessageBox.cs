using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpWpfMessageBox : MpINativeMessageBox {
        public bool ShowOkCancelMessageBox(string title, string message) {
            var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }

        public bool? ShowYesNoCancelMessageBox(string title, string message) {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel);
            if(result == MessageBoxResult.Yes) {
                return true;
            }
            if(result == MessageBoxResult.No) {
                return false;
            }
            return null;
        }
    }
}
