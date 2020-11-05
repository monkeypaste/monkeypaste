using GalaSoft.MvvmLight.Command;
using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAssignShortcutModalWindowViewModel : MpViewModelBase {
        #region Private Variables
        private bool _isSeqComplete = false;
        private bool _isNewCombination = true;

        private Window _windowRef = null;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;

        private object _parentRef = null;
        #endregion

        #region Properties
        private MpShortcut _duplicatedShortcut;
        public MpShortcut DuplicatedShortcut {
            get {
                return _duplicatedShortcut;
            }
            set {
                if (_duplicatedShortcut != value) {
                    _duplicatedShortcut = value;
                    OnPropertyChanged(nameof(DuplicatedShortcut));
                }
            }
        }

        private MpShortcut _shortcut;
        public MpShortcut Shortcut {
            get {
                return _shortcut;
            }
            set {
                if (_shortcut != value) {
                    _shortcut = value;
                    OnPropertyChanged(nameof(Shortcut));
                }
            }
        }

        private string _shortcutDisplayName = string.Empty;
        public string ShortcutDisplayName {
            get {
                return _shortcutDisplayName;
            }
            set {
                if(_shortcutDisplayName != value) {
                    _shortcutDisplayName = value;
                    OnPropertyChanged(nameof(ShortcutDisplayName));
                }
            }
        }

        private string _resetButtonText = string.Empty;
        public string ResetButtonText {
            get {
                return _resetButtonText;
            }
            set {
                if (_resetButtonText != value) {
                    _resetButtonText = value;
                    OnPropertyChanged(nameof(ResetButtonText));
                }
            }
        }

        private string _keysString = string.Empty;
        public string KeysString {
            get {
                return _keysString;
            }
            set {
                if(_keysString != value) {
                    _keysString = value;
                    OnPropertyChanged(nameof(KeysString));
                }
            }
        }

        private string _warningString = string.Empty;
        public string WarningString {
            get {
                return _warningString;
            }
            set {
                if (_warningString != value) {
                    _warningString = value;
                    OnPropertyChanged(nameof(WarningString));
                }
            }
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public void Init(MpShortcut shortcut) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(KeysString):
                        //when KeysString changes check full system for duplicates, ignoring order of combinations
                        WarningString = string.Empty;
                        //split hotkey into sequences combinations
                        var combos = Shortcut.KeyList.Split(',').ToList<string>();
                        //iterate over ALL shortcuts
                        foreach(MpShortcut sc in MpShortcut.GetAllShortcuts()) {
                            //ignore same shortcut comparision
                            if(sc.ShortcutId == Shortcut.ShortcutId) {
                                continue;
                            }
                            var scCombos = sc.KeyList.Split(',').ToList<string>();
                            if(combos.Count != scCombos.Count) {
                                continue;
                            }
                            int curComboIdx = 0;
                            bool isDuplicate = false;
                            foreach(string scCombo in scCombos) {
                                var scKeys = scCombo.Split('+').ToList<string>();
                                var keys = combos[curComboIdx].Split('+').ToList<string>();
                                if(keys.Count != scKeys.Count) {
                                    isDuplicate = false;
                                    break;
                                }
                                foreach(string k in keys) {
                                    if(scKeys.Contains(k)) {
                                        isDuplicate = true;
                                    } else {
                                        isDuplicate = false;
                                    }
                                }
                                curComboIdx++;
                            }
                            if (isDuplicate) {
                                DuplicatedShortcut = sc;
                                WarningString = "Warning! This combination conflicts with '" + sc.ShortcutName + "' which will be cleared if saved";
                            }
                        }
                        break;
                }
            };
            Shortcut = shortcut;
            ShortcutDisplayName = "'" + Shortcut.ShortcutName + "'";
            KeysString = Shortcut.KeyList;
            ResetButtonText = Shortcut.IsCustom() ? "Clear" : "Reset";
            _isSeqComplete = true;
        }
        public void AssignHotkeyModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            _windowRef.PreviewKeyDown += (s, e1) => {
                seqTimer.Stop();

                if (_isSeqComplete) {
                    Shortcut.ClearKeyList();
                    _isSeqComplete = false;
                    _isNewCombination = true;
                }
                int precount = Shortcut.KeyList.Length;
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) {
                    Shortcut.AddKey(Key.LeftCtrl,_isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
                    Shortcut.AddKey(Key.LeftCtrl, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
                    Shortcut.AddKey(Key.LeftShift, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightShift)) {
                    Shortcut.AddKey(Key.LeftShift, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftAlt)) {
                    Shortcut.AddKey(Key.LeftAlt, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightAlt)) {
                    Shortcut.AddKey(Key.LeftAlt, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LWin)) {
                    Shortcut.AddKey(Key.LWin, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RWin)) {
                    Shortcut.AddKey(Key.LWin, _isNewCombination && Shortcut.KeyList.Length == precount);
                }
                if(e1.Key != Key.LeftCtrl && 
                   e1.Key != Key.RightCtrl && 
                   e1.Key != Key.LeftAlt && 
                   e1.Key != Key.RightAlt && 
                   e1.Key != Key.LeftShift && 
                   e1.Key != Key.RightShift && 
                   e1.Key != Key.LWin && 
                   e1.Key != Key.RWin) {
                    if(Shortcut.KeyList.Length != precount) {
                        _isNewCombination = false;
                    }
                    Shortcut.AddKey(e1.Key, _isNewCombination);           
                } else {
                    _isNewCombination = false;
                }
            };

            _windowRef.PreviewKeyUp += (s, e1) => {
                _isNewCombination = true;
                KeysString = Shortcut.KeyList;
                seqTimer.Start();
            };

            seqTimer = new System.Timers.Timer(seqTimerMaxMs);
            seqTimer.AutoReset = true;
            seqTimer.Elapsed += (s, e2) => {
                _isSeqComplete = true;
            };
        }
        #endregion

        #region Commands
        private RelayCommand _cancelCommand;
        public ICommand CancelCommand {
            get {
                if(_cancelCommand == null) {
                    _cancelCommand = new RelayCommand(Cancel);
                }
                return _cancelCommand;
            }
        }
        private void Cancel() {
            Shortcut = null;
            _windowRef.Close();
        }

        private RelayCommand _resetCommand;
        public ICommand ResetCommand {
            get {
                if (_resetCommand == null) {
                    _resetCommand = new RelayCommand(Reset);
                }
                return _resetCommand;
            }
        }
        private void Reset() {
            if(!Shortcut.IsCustom()) {
                Shortcut.Reset();
                KeysString = Shortcut.KeyList;
            } else {
                Shortcut.ClearKeyList();
                KeysString = "[None]";
            }
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(Ok, CanOk);
                }
                return _okCommand;
            }
        }
        private bool CanOk() {
            return DuplicatedShortcut == null || DuplicatedShortcut.KeyList != DuplicatedShortcut.DefaultKeyList;
        }
        private void Ok() {
            if(KeysString == "[None]" || KeysString == string.Empty) {
                Shortcut = null;
            }
            if(DuplicatedShortcut != null) {

            }
            _windowRef.Close();
        }
        #endregion
    }
}
