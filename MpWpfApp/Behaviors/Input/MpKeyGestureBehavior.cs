using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MonkeyPaste;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpKeyGestureBehavior : MpBehavior<FrameworkElement> { 
        #region Private Variables

        private MpKeyGestureViewModel _gesture;

        private DateTime _startTime, _lastUpTime;

        private int _downCount = 0;
        private DispatcherTimer _timer;
        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            
        }

        public void StartListening(MpKeyGestureViewModel gesture) {
            _gesture = gesture;
            _startTime = DateTime.Now;
            _lastUpTime = DateTime.MaxValue;

            if(_timer == null) {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.IsEnabled = true;
                _timer.Tick += Timer_Elapsed;
            }
            _timer.Start();

            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.PreviewKeyUp += AssociatedObject_PreviewKeyUp;

        }

        public void StopListening() {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.PreviewKeyUp -= AssociatedObject_PreviewKeyUp;

            MessageBox.Show(_gesture.ToString());
        }

        private void Timer_Elapsed(object sender, EventArgs e) {
            if (_lastUpTime != DateTime.MaxValue && DateTime.Now - _lastUpTime > TimeSpan.FromSeconds(2)) {
                StopListening();
                _timer.Stop();
            }
        }


        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
            if (e.IsRepeat) {                
                return;
            }
            if(e.Key == Key.Enter) {
                StopListening();
            }
            
            DateTime downTime = DateTime.Now;
            MpKey k = new MpKey(e.Key);
            MpKeyViewModel key = _gesture.Keys.FirstOrDefault(x => x.Key.WpfKey == k.WpfKey);

            if(key == null) {
                key = new MpKeyViewModel(_gesture,k);
                _gesture.Keys.Add(key);
            }
            if((key.UpDownTimes.Count + 1) % 2 == 0) {
                MpConsole.WriteLine($"Key {key} up was not tracked");
            }

            key.UpDownTimes.Add(downTime);

            _downCount++;
        }

        private void AssociatedObject_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            DateTime upTime = DateTime.Now;
            MpKey k = new MpKey(e.Key);
            MpKeyViewModel key = _gesture.Keys.FirstOrDefault(x => x.Key.WpfKey == k.WpfKey);

            if (key == null) {
                MpConsole.WriteLine($"Key {key} down was not tracked");
                return;
            }

            if (key.UpDownTimes.Count % 2 == 0) {
                MpConsole.WriteLine($"Key {key} down was not tracked");
            }

            key.UpDownTimes.Add(upTime);

            _downCount--;

            _lastUpTime = DateTime.Now;
        }

        
    }
}
