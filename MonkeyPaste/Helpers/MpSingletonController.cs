﻿using Microsoft.Win32;
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
        private Dictionary<string,MpKeyboardHook> _hotkeyHookDictionary;
        private MpData _data;

        private MpSettings _settings { get; set; }
        public MpSettings Settings { get { return _settings; } set { _settings = value; } }

        private int _scrollWheelDelta { get; set; }
        public int ScrollWheelDelta { get { return _scrollWheelDelta; } set { _scrollWheelDelta = value; } }

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
        public void Init(object context,string dbPath,string dbPassword,string idToken,string accessToken) {
            _appContext = context;
            Rand = new Random(Convert.ToInt32(DateTime.Now.Second));
            _settings = new MpSettings();
            _data = new MpData(dbPath,dbPassword,idToken,accessToken);
            _scrollWheelDelta = 0;       
            _hotkeyHookDictionary = new Dictionary<string,MpKeyboardHook>();
            _data.Init();
        }
        public void ExitApplication() {            
            ((MpApplicationContext)_appContext).ExitCore();
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
            _settings.SetSetting(key,val,Settings.SettingValueTypeDictionary[key]);
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