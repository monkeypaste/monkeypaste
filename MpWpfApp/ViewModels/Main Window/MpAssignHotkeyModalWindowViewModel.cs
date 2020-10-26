using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAssignHotkeyModalWindowViewModel : MpViewModelBase {
        #region Private Variables
        private ICommand _hotkeyCommand = null;

        private MpHotKeyItem _currentHotKeyItem = null;

        private bool _isSeqComplete = false;
        private bool _isLShiftDown = false;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;
        #endregion

        #region Properties
        private MpCommand _command;
        public MpCommand Command {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        private string _commandName = string.Empty;
        public string CommandName {
            get {
                return _commandName;
            }
            set {
                if(_commandName != value) {
                    _commandName = value;
                    OnPropertyChanged(nameof(CommandName));
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
        public void Init(ICommand hotkeyCommand, string commandName) {
            _hotkeyCommand = hotkeyCommand;
            CommandName = "'" + commandName + "'";
            Command = new MpCommand();
            Command.CommandRef = hotkeyCommand;
            KeysString = "<None>";
        }
        public void AssignHotkeyModalWindow_Loaded(object sender, RoutedEventArgs e) {
            var ahmw = (Window)sender;
            ahmw.PreviewKeyDown += (s, e1) => {
                seqTimer.Stop();

                if (_isSeqComplete) {
                    Command.ClearHotKeyList();
                    _isSeqComplete = false;
                    _currentHotKeyItem = null;
                }
                if (_currentHotKeyItem == null) {
                    _currentHotKeyItem = new MpHotKeyItem();
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) {
                    _currentHotKeyItem.AddKey(Key.LeftCtrl);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
                    _currentHotKeyItem.AddKey(Key.LeftCtrl);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
                    _currentHotKeyItem.AddKey(Key.LeftShift);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightShift)) {
                    _currentHotKeyItem.AddKey(Key.LeftShift);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftAlt)) {
                    _currentHotKeyItem.AddKey(Key.LeftAlt);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightAlt)) {
                    _currentHotKeyItem.AddKey(Key.LeftAlt);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LWin)) {
                    _currentHotKeyItem.AddKey(Key.LWin);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RWin)) {
                    _currentHotKeyItem.AddKey(Key.LWin);
                }
                _currentHotKeyItem.AddKey(e1.Key);
            };

            ahmw.PreviewKeyUp += (s, e1) => {
                if(_currentHotKeyItem != null) {
                    Command.HotKeyItemList.Add(_currentHotKeyItem);
                    _currentHotKeyItem = null;
                    KeysString = Command.GetHotKeyString();
                    Console.WriteLine("KeyString: " + KeysString);
                    
                }
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
            Command.ClearHotKeyList();
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
            Command.WriteToDatabase();
        }
        #endregion
    }
}
