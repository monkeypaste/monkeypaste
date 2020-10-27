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
    public class MpAssignHotkeyModalWindowViewModel : MpViewModelBase {
        #region Private Variables
        private bool _isSeqComplete = false;
        private bool _isNewCombination = true;

        private Window _windowRef = null;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;

        private object _parentRef = null;
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
        public void Init(ICommand hotkeyCommand, string commandName,object parent,int copyItemId = 0) {
            CommandTypeName = "'" + commandName + "'";
            Command = new MpCommand();
            //Command.CommandRef = hotkeyCommand;
            Command.CopyItemId = copyItemId;
            KeysString = "<None>";
            _parentRef = parent;
        }
        public void AssignHotkeyModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            _windowRef.PreviewKeyDown += (s, e1) => {
                seqTimer.Stop();

                if (_isSeqComplete) {
                    Command.ClearHotKeyList();
                    _isSeqComplete = false;
                    _isNewCombination = true;
                }
                //bool isModKey = true;
                //if ((e1.Key != Key.LeftCtrl && e1.Key != Key.RightCtrl && e1.Key != Key.LeftAlt && e1.Key != Key.RightAlt && e1.Key != Key.LeftShift && e1.Key != Key.RightShift && e1.Key != Key.LWin && e1.Key != Key.RWin)) {
                //    isModKey = false;
                //}
                int precount = Command.KeyList.Length;
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) {
                    Command.AddKey(Key.LeftCtrl,_isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightCtrl)) {
                    Command.AddKey(Key.LeftCtrl, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftShift)) {
                    Command.AddKey(Key.LeftShift, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightShift)) {
                    Command.AddKey(Key.LeftShift, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LeftAlt)) {
                    Command.AddKey(Key.LeftAlt, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RightAlt)) {
                    Command.AddKey(Key.LeftAlt, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.LWin)) {
                    Command.AddKey(Key.LWin, _isNewCombination && Command.KeyList.Length == precount);
                }
                if (e1.KeyboardDevice.IsKeyDown(Key.RWin)) {
                    Command.AddKey(Key.LWin, _isNewCombination && Command.KeyList.Length == precount);
                }
                if(e1.Key != Key.LeftCtrl && 
                   e1.Key != Key.RightCtrl && 
                   e1.Key != Key.LeftAlt && 
                   e1.Key != Key.RightAlt && 
                   e1.Key != Key.LeftShift && 
                   e1.Key != Key.RightShift && 
                   e1.Key != Key.LWin && 
                   e1.Key != Key.RWin) {
                    if(Command.KeyList.Length != precount) {
                        _isNewCombination = false;
                    }
                    Command.AddKey(e1.Key, _isNewCombination);           
                } else {
                    _isNewCombination = false;
                }
            };

            _windowRef.PreviewKeyUp += (s, e1) => {
                //Command.HotKeyList.Add(_currentHotKey);
                _isNewCombination = true;
                KeysString = Command.KeyList;
                Console.WriteLine("KeyString: " + KeysString);

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
            Command = null;
            _windowRef.Close();
            ((MpClipTileViewModel)_parentRef).ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
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
            //Command.RegisterCommand();
            Command.WriteToDatabase();
            Console.WriteLine("Successfully created: " + Command.ToString());
            _windowRef.Close();
            
            ((MpClipTileViewModel)_parentRef).ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
        }
        #endregion
    }
}
