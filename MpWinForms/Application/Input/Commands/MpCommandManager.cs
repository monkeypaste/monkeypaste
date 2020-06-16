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
    public enum MpMouseEventType {
        NONE=0,
        LBUTTON,
        RBUTTON,
        SCROLLUP,
        SCROLLDOWN,
        SCROLLCLICK,
        MOVE
    }
    public enum MpAppStateType {
        NONE=0,
        INIT_WELCOME, //Shows slideshow of tips w/ checkbox to show on startup
        INIT_LOGIN,   //gather user cred's or load fresh db
        INIT_SPLASH, //show splash center screen with progress bar loading data
        INIT_FAIL,  //overlays splash and returns to login or terminates
        INIT_SUCCESS, //optionally shows intro bubble 
        BG_INACTIVE, //when clipboard monitoring and global commands disabled
        BG_ACTIVE,  //when no forms are visible but app is loaded and listening
        LOG_FOCUS, //when log is first showeds
    }
    //public class MpInputContext {
    //    public Type FocusedUIControlType { get; set; }
    //    public Type FocusedControllerType { get; set; }
    //    public MpInputContext(Type focusedUIControlType, Type focusedControllerType = null) {
    //        FocusedUIControlType = focusedUIControlType;
    //        FocusedControllerType = focusedControllerType;
    //    }
    //}
    //public class MpAppState {
    //    public MpAppStateType AppState { get; set; }
    //    public MpInputContext AppFocus { get; set; }
    //    public MpAppState(MpAppStateType ast,MpInputContext af) {
    //        AppState = ast;
    //        AppFocus = af;
    //    }
    //}

    public class MpCommandManager  {
        private static readonly Lazy<MpCommandManager> lazy = new Lazy<MpCommandManager>(() => new MpCommandManager());
        public static MpCommandManager Instance { get { return lazy.Value; } }

        public MpClipboardManager ClipboardCommander { get; set; }

        private KeyboardHookManager _globalKeyboardManager;
        private IKeyboardMouseEvents _inputEvent;
        //private MpKeyboardHook _toggleLogHook, _toggleAppendMode, _toggleAutoCopyMode;
        //private IKeyboardMouseEvents _clickHook, _moveHook, _mouseDownHook, _mouseUpHook;

        private ObservableCollection<MpCommand> _commandList = new ObservableCollection<MpCommand>();

        private List<MpCommand> _registeredCommandList = new List<MpCommand>();
        private List<MpCommand> _unregisteredCommandList = new List<MpCommand>();

        public MpCommandManager() {
            _commandList = new ObservableCollection<MpCommand>();

            _globalKeyboardManager = new KeyboardHookManager();
            _globalKeyboardManager.Start();

            _inputEvent = Hook.GlobalEvents();
            _inputEvent.KeyPress += _inputEvent_KeyPress;

            ClipboardCommander = new MpClipboardManager();
            ClipboardCommander.Init();
        }

        private void _inputEvent_KeyPress(object sender, KeyPressEventArgs e) {
           foreach(MpCommand cmd in _commandList) {
            }
        }

        public void RegisterCommand(MpCommand cmd, Action handler) {
            if (cmd.IsGlobal) {
                //_globalKeyboardManager.RegisterHotkey((ModifierKeys[])cmd.HotKey.ModKeyList.ToArray(), cmd.HotKey.KeyCode, handler);
            } else {
                if(cmd.CommandView == null) {
                    Console.WriteLine("Error MpCommandManager trying to register a non-global command without specifiying a view for th input, Command: " + cmd.ToString());
                    return;
                }

            }
        }
        
    }
}
