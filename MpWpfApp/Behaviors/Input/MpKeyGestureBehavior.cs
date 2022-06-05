using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MonkeyPaste;
using System.Windows.Threading;
using System.Diagnostics;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpKeyGestureBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables
        private List<KeyValuePair<Key, DateTime>> _downs = new List<KeyValuePair<Key, DateTime>>();
        private List<KeyValuePair<Key, DateTime>> _ups = new List<KeyValuePair<Key, DateTime>>();

        private MpAssignShortcutModalWindowViewModel _asmwvm;

        //private MpKeyGestureViewModel _gesture;

        private DateTime _lastUpTime;

        private int _downCount = 0;
        private DispatcherTimer _timer;
        #endregion

        protected override void OnLoad() {
            base.OnLoad();
            
        }

        public void StartListening(MpAssignShortcutModalWindowViewModel asmwvm) {
            _asmwvm = asmwvm;
            _asmwvm.OnClear += _asmwvm_OnClear;
            //_gesture = new MpKeyGestureViewModel(_asmwvm.KeyString);


            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.IsEnabled = true;
                _timer.Tick += Timer_Elapsed;
            }
            Reset();
            _timer.Start();

            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.PreviewKeyUp += AssociatedObject_PreviewKeyUp;

        }

        private void _asmwvm_OnClear(object sender, EventArgs e) {
            Reset();
        }

        private void Reset() {
            _timer.Stop();
            _ups.Clear();
            _downs.Clear();

            _lastUpTime = DateTime.MaxValue;
        }

        public void StopListening() {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.PreviewKeyUp -= AssociatedObject_PreviewKeyUp;
            _timer.Tick -= Timer_Elapsed;
            Reset();
        }

        private void Timer_Elapsed(object sender, EventArgs e) {   
            string gesture = GetGesture();
            var keylist = MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(gesture);
            _asmwvm.SetKeyList(keylist);
        }


        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
            if(!_timer.IsEnabled) {
                _timer.Start();
            }
            if (e.IsRepeat) {                
                return;
            }            
            if(_lastUpTime != DateTime.MaxValue && DateTime.Now - _lastUpTime > TimeSpan.FromSeconds(2)) {
                Reset();
            }

            _downs.Add(new KeyValuePair<Key, DateTime>(e.Key, DateTime.Now));
            _downCount++;

            string gesture = GetGesture();
            var keylist = MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(gesture);
            _asmwvm.SetKeyList(keylist);
        }

        private void AssociatedObject_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            _ups.Add(new KeyValuePair<Key,DateTime>(e.Key, DateTime.Now));
            _downCount--;

            if(_downCount == 0) {
                string gesture = GetGesture();
                var keylist = MpWpfKeyboardInputHelpers.ConvertStringToKeySequence(gesture);
                _asmwvm.SetKeyList(keylist);
            }
            _lastUpTime = DateTime.Now;
        }

        public string GetGesture() {
            if(_ups.Count == 0 && _downs.Count == 0) {
                return _asmwvm.KeyString;
            }
            var ups = _ups.Select(x => x).ToList().OrderBy(x => x.Value).ToList();
            var downs = _downs.Select(x => x).ToList().OrderBy(x => x.Value).ToList();

            TimeSpan minUniqueUpSpan = TimeSpan.FromMilliseconds(100);
            DateTime lastUp = DateTime.MinValue;
            List<string> combos = new List<string>();
            
            for (int i = 0; i < ups.Count ; i++) {
                if (i > 0) {
                    //remove last up from downs
                    var downsToRemove = downs.Where(d => 
                                                d.Value < ups[i - 1].Value 
                                                && d.Key == ups[i - 1].Key
                                                ).ToList();
                    for (int j = 0; j < downsToRemove.Count; j++) {
                        downs.Remove(downsToRemove[j]);
                    }
                }
                
                DateTime upTime = ups[i].Value;
                
                var combo = downs.Where(d => d.Value < upTime)
                                  .Select(d => d.Key)
                                  .ToList().OrderBy(x => GetPriority(x));
                combos.Add(string.Join("+", combo.Select(x => x.ToString())));
                if (downs.All(d => d.Value < upTime)) {
                    break;
                }
            }

            if (combos.Count == 0 && downs.Count > 0 && _ups.Count == 0) {
                //mainly for realtime feedback
                combos.Add(string.Join("+", downs.Select(x => x.Key).OrderBy(x => GetPriority(x))));
            }

            return string.Join(", ", combos);
        }

        private int GetPriority(Key key) {
            switch (key) {
                case Key.LeftCtrl:
                    return 0;
                case Key.RightCtrl:
                    return 1;
                case Key.System:
                case Key.LeftAlt:
                    return 2;
                case Key.RightAlt:
                    return 3;
                case Key.LeftShift:
                    return 4;
                case Key.RightShift:
                    return 5;
                default:
                    return 6;
            }
        }
    }
}
