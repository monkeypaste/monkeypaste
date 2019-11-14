using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSingletonController {
        private static readonly Lazy<MpSingletonController> lazy = new Lazy<MpSingletonController>(() => new MpSingletonController());
        public static MpSingletonController Instance { get { return lazy.Value; } }

        //public MpInputEventType InputEvent = new MpInputEventType();
        //private Dictionary<string,MpKeyboardHook> _hotkeyHookDictionary;
        //private MpData _data;

        public int ScrollWheelDelta { get; set; } = 0;

        public MpRegistryHelper Rh { get; set; } = new MpRegistryHelper();

        public MpCopyItem AppendItem { get; set; }
        public bool InAppendMode { get; set; } = false;

        public bool InCopySelectionMode { get; set; } = false;

        public bool InRightClickPasteMode { get; set; } = false;

        private bool _ignoreNextClipboardEvent;

        private object _appContext;

        public Random Rand { get; set; }

        public float TileTitleFontSize { get; set; }

        public int CustomLogHeight { get; set; }

        public void ScrollWheelListener(object sender, MouseEventArgs e) {
            ScrollWheelDelta += e.Delta;
        }
        public void Init(object context) {
            _appContext = context;
            Rand = new Random(Convert.ToInt32(DateTime.Now.Second));
            //_data = new MpData(dbPath,dbPassword,idToken,accessToken);
            //_hotkeyHookDictionary = new Dictionary<string,MpKeyboardHook>();
            //_data.Init();
        }
        //public MpKeyboardHook GetKeyboardHook(string key) {
        //    if(_hotkeyHookDictionary.ContainsKey(key)) {
        //        return _hotkeyHookDictionary[key];
        //    }
        //    Console.WriteLine("Error, no hotkey found for " + key);
        //    return null;
        //}
        //public void SetKeyboardHook(string key,MpKeyboardHook newHook) {
        //    if(_hotkeyHookDictionary.ContainsKey(key)) {
        //        _hotkeyHookDictionary[key] = newHook;
        //    }
        //    else {
        //        _hotkeyHookDictionary.Add(key,newHook);
        //    }
        //}
        
        //public MpData GetMpData() {
        //    return _data;
        //}
        public void ExitApplication() {
            ((MpApplicationContext)_appContext).ExitCore();
        }
        
        public bool GetIgnoreNextClipboardEvent() {
            return _ignoreNextClipboardEvent;
        }
        public void SetIgnoreNextClipboardEvent(bool ignore) {
            _ignoreNextClipboardEvent = ignore;
        }
    }
}
