using Gma.System.MouseKeyHook;
using NonInvasiveKeyboardHookLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {

    public class MpCommandManager  {
        private static readonly Lazy<MpCommandManager> lazy = new Lazy<MpCommandManager>(() => new MpCommandManager());
        public static MpCommandManager Instance { get { return lazy.Value; } }

        public MpClipboardManager ClipboardCommander { get; set; }

        private KeyboardHookManager _globalKeyboardManager;
        private IKeyboardMouseEvents _inputEvent;

        public MpCommandManager() {
            _globalKeyboardManager = new KeyboardHookManager();
            _globalKeyboardManager.Start();

            _inputEvent = Hook.GlobalEvents();

            ClipboardCommander = new MpClipboardManager();
            ClipboardCommander.Init();
        }
    }
}
