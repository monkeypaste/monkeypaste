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
        private MpShortcut _command;
        public MpShortcut Shortcut {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Shortcut));
                }
            }
        }

        private string _commandName = string.Empty;
        public string CommandTypeName {
            get {
                return _commandName;
            }
            set {
                if(_commandName != value) {
                    _commandName = value;
                    OnPropertyChanged(nameof(CommandTypeName));
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
        #endregion

        #region Private Methods
        
        #endregion

        #region Public Methods
        public void Init(MpShortcut shortcut) {
            Shortcut = shortcut;
            CommandTypeName = "'" + Shortcut.ShortcutName + "'";
            KeysString = Shortcut.KeyList;
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
            Shortcut.ClearKeyList();
            KeysString = "[None]";
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(Ok);
                }
                return _okCommand;
            }
        }
        private void Ok() {
            Console.WriteLine("Successfully created: " + Shortcut.ToString());
            _windowRef.Close();
        }
        #endregion
    }
}
