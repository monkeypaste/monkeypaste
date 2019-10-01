using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    //    public class MpInputEvent2 {
    //        public string ToggleLog = "ToggleLog";
    //        public string HideLog = "HideLog";
    //        public string SelectLeftTile = "SelectLeftTile";
    //        public string SelectRightTile = "SelectRightTile";
    //        public string PasteTile = "PasteTile";
    //        public string EditTile = "EditTile";
    //        public string ToggleSettings = "ToggleSettings";
    //    }
    public class MpSingletonController {
        private static readonly Lazy<MpSingletonController> lazy = new Lazy<MpSingletonController>(() => new MpSingletonController());
        public static MpSingletonController Instance { get { return lazy.Value; } }

        //public MpInputEventType InputEvent = new MpInputEventType();
        private Dictionary<string,MpKeyboardHook> _hotkeyHookDictionary;
        private MpData _data;
        private MpSettings _settings;
        private bool _ignoreNextClipboardEvent;
        private int _scrollWheelDelta { get; set; }
        public int ScrollWheelDelta { get { return _scrollWheelDelta; } set { _scrollWheelDelta = value; } }
        
        public void ScrollWheelListener(object sender, MouseEventArgs e) {
            ScrollWheelDelta += e.Delta;
        }
        public void Init(string dbPath,string dbPassword,string idToken,string accessToken) {
            _data = new MpData(dbPath,dbPassword,idToken,accessToken);
            _scrollWheelDelta = 0;
            _settings = new MpSettings();            
            _hotkeyHookDictionary = new Dictionary<string,MpKeyboardHook>();
            _data.Init();
        }
        public MpKeyboardHook GetKeyboardHook(string key) {
            if(_hotkeyHookDictionary.ContainsKey(key)) {
                return _hotkeyHookDictionary[key];
            }
            Console.WriteLine("Error, no hotkey found for " + key);
            return null;
        }
        public void SetKeyboardHook(string key,MpKeyboardHook newHook) {
            if(_hotkeyHookDictionary.ContainsKey(key)) {
                _hotkeyHookDictionary[key] = newHook;
            }
            else {
                _hotkeyHookDictionary.Add(key,newHook);
            }
        }
        public object GetSetting(string key) {
            return _settings.GetSetting(key);
        }
        public void SetSetting(string key,object val) {
            _settings.SetSetting(key,val);
        }
        
        public MpData GetMpData() {
            return _data;
        }
        public bool GetIgnoreNextClipboardEvent() {
            return _ignoreNextClipboardEvent;
        }
        public void SetIgnoreNextClipboardEvent(bool ignore) {
            _ignoreNextClipboardEvent = ignore;
        }
    }
}
