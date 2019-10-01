using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MonkeyPaste {

    public class MpInputCommand {
        public static string ToggleLog = "ToggleLog";
        public static string HideLog = "HideLog";
        public static string SelectLeftTile = "SelectLeftTile";
        public static string SelectRightTile = "SelectRightTile";
        public static string PasteTile = "PasteTile";
        public static string EditTile = "EditTile";
        public static string ToggleSettings = "ToggleSettings";
        public static string MinimizeTileSize = "MinimizeTileSize";
        public static string RestoreTileSize = "RestoreTileSize";
        public static string ExpandTileSize = "ExpandTileSize";
    }
    public class MpKeyboardHotKey {
        //public MpComman
        public KeyValuePair<List<ModifierKeys>,List<Keys>> hotKeyValuePair { get; set; }        
    }
    public class MpKeyboardController {
        
        private Dictionary<MpInputEventType,MpKeyboardHotKey> _keyboardkDictionary;

        public MpKeyboardController() {
        }

        private Keys _keyCode;

        public void MpKeyboardController_KeyDown(object sender,KeyEventArgs e) {
            _keyCode = e.KeyCode;
            Console.WriteLine("KeyEventArgs: " + e.ToString());
        }
    }


    public class MpMouseEvent {

        private List<MouseButtons> _mouseButtonList = new List<MouseButtons>();

        public MpMouseEvent(List<MouseButtons> mbl,bool trackMotion,string ename) {
            _mouseButtonList = mbl;
            if(mbl == null || mbl.Count == 0) {
                m_GlobalHook = Hook.GlobalEvents();
                m_GlobalHook.MouseMove += M_GlobalHook_MouseMove;

            }
            EventName = ename;
            Console.WriteLine("Mouse event '" + EventName + "' created w/ buttons: " + _mouseButtonList.ToString());
        }

        private void M_GlobalHook_MouseMove(object sender,MouseEventArgs e) {
            throw new NotImplementedException();
        }
        public string EventName = "Unknown";
        private MpInputEventType _eventType;

        protected IKeyboardMouseEvents m_GlobalHook;

        public void SetEventType(MpInputEventType et) {
            _eventType = et;
        }
        public MpInputEventType GetEventType() {
            return _eventType;
        }

        public void Subscribe() {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;

            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }
        public void Unsubscribe() {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }
        private void GlobalHookKeyPress(object sender,KeyPressEventArgs e) {
            Console.WriteLine("KeyPress: \t{0}",e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender,MouseEventExtArgs e) {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}",e.Button,e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }
    }
    public enum MpInputEventType {
        PressBegin,
        PressEnd,
        Press
    }
}
