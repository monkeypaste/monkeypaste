using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCommand {
        public string Name { get; set; } = string.Empty;

        public bool IsGlobal { get; set; } = false;

        public MpHotKey HotKey = new MpHotKey();
        public List<MpMouseEventType> MouseList = new List<MpMouseEventType>();

        public delegate void CommandExecuted(object sender);
        public event CommandExecuted CommandExecutedEvent;

        public MpController CommandController { get; set; }

        public Control CommandView { get; set; }

        public Action CommandAction { get; set; }

        public MpCommand(
            string name,
            int keyCode,
            List<ModifierKeys> modKeyList,
            List<MpMouseEventType> mouseList,
            MpController focusController,
            Control focusView,
            bool isGlobal,
            Action commandExecutted
        ) {            
            Name = name;
            HotKey = new MpHotKey(keyCode, modKeyList);
            MouseList = mouseList;
            CommandController = focusController;
            CommandView = focusView;
            IsGlobal = isGlobal;
            CommandAction = commandExecutted;
        }
        public bool CanCommandExecute() {

            return false;
        }
        public void OnCommandExecuted() {
            CommandExecutedEvent(this);
            Console.WriteLine("Executed: " + ToString());
        }
        public override string ToString() {
            return "Command Name: " + Name + " IsGlobal: " + Convert.ToInt32(IsGlobal) + " Keys: " + HotKey.ToString();
        }
    }
}
