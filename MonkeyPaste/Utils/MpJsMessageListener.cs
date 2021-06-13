using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MonkeyPaste {
    public class MpJsMessageListener {
        #region Private Variables
        private Timer _msgListenerTimer = null;
        private double _timerInterval = 300;
        private bool _isStopping = false;

        private Func<string, Task<string>> _evaluateJavascript;
        #endregion

        #region Properties
        public ObservableCollection <MpJsMessage> Messages { get; set; }        
        #endregion

        #region Event Definitions
        public event EventHandler<MpJsMessage> OnMessageReceived;
        #endregion

        #region Public Methods
        public MpJsMessageListener(Func<string, Task<string>> jsEvaluator) {
            _evaluateJavascript = jsEvaluator;
            Messages = new ObservableCollection<MpJsMessage>();

            _msgListenerTimer = new System.Timers.Timer() {
                Interval = _timerInterval,
                AutoReset = false
            };
            _msgListenerTimer.Elapsed += _msgListenerTimer_Elapsed;
        }

        public void Start() {
            _msgListenerTimer.Start();
        }

        public void Stop() {
            _isStopping = true;
        }

        public void Reset() {
            Messages.Clear();
        }
        
        #endregion

        #region Private Methods
        private async Task CheckOutMessages() {
            var newMessageString = await _evaluateJavascript($"PopOutMessage()");
            newMessageString.Replace("\"", string.Empty);
            MpConsole.WriteLine($"Message Received {DateTime.UtcNow}: {newMessageString}");
            if(_isStopping) {
                _isStopping = false;
                return;
            }
            _msgListenerTimer.Start();
        }
        #region Event Handlers
        private void _msgListenerTimer_Elapsed(object sender, ElapsedEventArgs e) {
            CheckOutMessages();
        }
        #endregion

        #endregion
    }
}
