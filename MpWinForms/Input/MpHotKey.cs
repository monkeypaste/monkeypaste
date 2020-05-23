using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpHotKey {
        public List<ModifierKeys> ModKeyList = new List<ModifierKeys>();
        public int KeyCode { get; set; } = (int)Keys.None;

        public MpHotKey(int keyCode, List<ModifierKeys> modKeyList = null) {
            ModKeyList = modKeyList == null ? new List<ModifierKeys>() : modKeyList;
            KeyCode = keyCode;
        }
        public MpHotKey() : this(0) { }

        public override string ToString() {
            string outStr = "";
            foreach(ModifierKeys modKey in ModKeyList) {
                if(modKey == ModifierKeys.Alt) {
                    outStr += "Alt+";
                }
                else if(modKey == ModifierKeys.Control) {
                    outStr += "Ctrl+";
                }
                else if(modKey == ModifierKeys.Shift) {
                    outStr += "Shift+";
                }
                else if(modKey == ModifierKeys.Win) {
                    outStr += "Win+";
                }
            }
            return outStr + "+" + Enum.GetName(typeof(Keys), (Keys)KeyCode);
        }
    }
}
