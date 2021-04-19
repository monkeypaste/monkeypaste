using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpShortcutInputMonitor {
        #region Private Variables
        private bool _isSeqComplete = false;
        private bool _isNewCombination = true;

        private System.Timers.Timer seqTimer = null;
        private double seqTimerMaxMs = 1000;

        private string _keys = string.Empty;

        #endregion

        #region Properties

        private ObservableCollection<MpShortcutKeyViewModel> _keyItems = new ObservableCollection<MpShortcutKeyViewModel>();
        public ObservableCollection<MpShortcutKeyViewModel> KeyItems {
            get {
                return _keyItems;
            }
            set {
                if (_keyItems != value) {
                    _keyItems = value;
                }
            }
        }
        public string KeyString {
            get {
                KeyItems.Clear();
                foreach (var kl in _keyList) {
                    foreach (var k in kl) {
                        if (kl.Count > 1 && kl.IndexOf(k) < kl.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            true, false));
                        } else if (kl.IndexOf(k) == kl.Count - 1 && _keyList.IndexOf(kl) < _keyList.Count - 1) {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, true));
                        } else {
                            KeyItems.Add(new MpShortcutKeyViewModel(
                                            MpHelpers.Instance.GetKeyLiteral(k),
                                            false, false));
                        }

                    }
                }
                return MpHelpers.Instance.ConvertKeySequenceToString(_keyList);
            }
        }
        private List<List<Key>> _keyList = new List<List<Key>>();
        public List<List<Key>> KeyList {
            get {
                return _keyList;
            }
            set {
                if(_keyList != value) {
                    _keyList = value;                    
                }
            }
        }
        #endregion

        #region Public Methods
        public MpShortcutInputMonitor() {
            seqTimer = new System.Timers.Timer(seqTimerMaxMs);
            seqTimer.AutoReset = true;
            seqTimer.Elapsed += (s, e2) => {
                _isSeqComplete = true;
            };
            _isSeqComplete = true;
        }

        public void InputMonitor_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e1) {
            seqTimer.Stop();

            if (_isSeqComplete) {
                Clear();
            }
            int precount = KeyString.Length; 
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.LeftCtrl)) {
                AddKey(Key.LeftCtrl, _isNewCombination && KeyString.Length == precount);
            }
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.RightCtrl)) {
                AddKey(Key.LeftCtrl, _isNewCombination && KeyString.Length == precount);
            }
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.LeftShift)) {
                AddKey(Key.LeftShift, _isNewCombination && KeyString.Length == precount);
            }
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.RightShift)) {
                AddKey(Key.LeftShift, _isNewCombination && KeyString.Length == precount);
            }
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.LeftAlt)) {
                AddKey(Key.LeftAlt, _isNewCombination && KeyString.Length == precount);
            }
            if (Keyboard.PrimaryDevice.IsKeyDown(Key.RightAlt)) {
                AddKey(Key.LeftAlt, _isNewCombination && KeyString.Length == precount);
            }
            Key key = MpHelpers.Instance.WinformsToWPFKey(e1.KeyCode);
            //if keyboardDevice.IsKeyDown(Key.LWin)) {
            //    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
            //}
            //if keyboardDevice.IsKeyDown(Key.RWin)) {
            //    AddKey(Key.LWin, _isNewCombination && KeysString.Length == precount);
            //}
            if(key != Key.LeftCtrl &&
               key != Key.RightCtrl &&
               key != Key.LeftAlt &&
               key != Key.RightAlt &&
               key != Key.LeftShift &&
               key != Key.RightShift /*&&
                   key != Key.LWin &&
                   key != Key.RWin*/) {
                if (KeyString.Length != precount) {
                    _isNewCombination = false;
                }
                AddKey(key, _isNewCombination);
            } else {
                _isNewCombination = false;
            }

            string outStr = string.Empty;
            foreach(var combo in KeyList) {
                foreach(var k in combo) {
                    outStr += k.ToString() + "+";
                }
                outStr += ",";
            }
            Console.WriteLine("Current Input: "+outStr);
        }

        public void InputMonitor_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e1) {
            _isNewCombination = true;
            seqTimer.Start();
        }
        #endregion

        #region Private Methods
        private void AddKey(Key key, bool isNewCombination) {
            if (isNewCombination && KeyString.Length > 0) {
                //KeysString += ",";
                KeyList.Add(new List<Key>());
            }
            if (KeyList.Count == 0) {
                KeyList.Add(new List<Key>());
            }
            if (!KeyList[KeyList.Count - 1].Contains(key)) {
                KeyList[KeyList.Count - 1].Add(key);
            }

            for (int i = 0; i < KeyList.Count; i++) {
                var kl = KeyList[i];
                if (kl.Contains(Key.LeftShift)) {
                    kl.Remove(Key.LeftShift);
                    kl.Insert(0, Key.LeftShift);
                }
                if (kl.Contains(Key.LeftAlt)) {
                    kl.Remove(Key.LeftAlt);
                    kl.Insert(0, Key.LeftAlt);
                }
                if (kl.Contains(Key.LeftCtrl)) {
                    kl.Remove(Key.LeftCtrl);
                    kl.Insert(0, Key.LeftCtrl);
                }
                KeyList[i] = kl;
            }
        }

        private void Clear() {
            _isSeqComplete = false;
            _isNewCombination = true;
            KeyList.Clear();
            KeyItems.Clear();
        }
        #endregion
    }
}
