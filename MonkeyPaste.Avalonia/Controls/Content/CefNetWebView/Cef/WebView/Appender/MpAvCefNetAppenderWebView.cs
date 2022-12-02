using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetAppenderWebView : MpAvCefNetWebView {
        #region Private Variables

        private const int _APPEND_TIMEOUT_MS = 5000;
        private string _pendingAppendData;
        #endregion

        #region Statics

        public static string APPEND_NOTIFIER_PARAMS => "append_notifier=true";
        #endregion

        #region Properties
        public override string ContentUrl => base.ContentUrl + $"?{APPEND_NOTIFIER_PARAMS}";

        #region AppendModeState Property

        private bool? _appendModeState;
        public bool? AppendModeState {
            get { return _appendModeState; }
            set { SetAndRaise(AppendModeStateProperty, ref _appendModeState, value); }
        }

        public static DirectProperty<MpAvCefNetAppenderWebView, bool?> AppendModeStateProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetAppenderWebView, bool?>(
                nameof(AppendModeState),
                x => x.AppendModeState,
                (x, o) => x.AppendModeState = o,
                null,
                BindingMode.TwoWay);
        private void OnAppendModeStateChanged() {
            if (BindingContext == null) {
                return;
            }

            MpConsole.WriteLine($"AppendModeState changed. AppendModeState: {AppendModeState}");
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                while (!IsContentLoaded) {
                    await Task.Delay(100);
                    if (sw.ElapsedMilliseconds > _APPEND_TIMEOUT_MS) {
                        //timeout, content changed never notified back
                        Debugger.Break();
                        break;
                    }
                }

                if (AppendModeState.HasValue) {
                    var reqMsg = new MpQuillAppendModeEnabledRequestMessage() {
                        isAppendLineMode = AppendModeState.Value
                    };
                    this.ExecuteJavascript($"appendModeEnabled_ext('{reqMsg.SerializeJsonObjectToBase64()}')");
                } else {
                    this.ExecuteJavascript($"appendModeDisabled_ext()");
                }
            });
        }
        #endregion HasAppendModel Property

        #region AppendData Property

        private string _appendData;
        public string AppendData {
            get { return _appendData; }
            set { SetAndRaise(AppendDataProperty, ref _appendData, value); }
        }

        public static DirectProperty<MpAvCefNetAppenderWebView, string> AppendDataProperty =
            AvaloniaProperty.RegisterDirect<MpAvCefNetAppenderWebView, string>(
                nameof(AppendData),
                x => x.AppendData,
                (x, o) => x.AppendData = o,
                null,
                BindingMode.TwoWay);

        private async void OnAppendDataChanged() {
            if (BindingContext == null ||
                AppendData == null) {
                return;
            }

            MpConsole.WriteLine($"AppendData changed. AppendData: {AppendData}");
            var req = new MpQuillAppendDataRequestMessage() { appendData = AppendData };
            this.ExecuteJavascript($"appendData_ext('{req.SerializeJsonObjectToBase64()}')");

            AppendData = null;
            IsContentLoaded = false;
            if (this.GetVisualAncestor<Window>() is Window w &&
                w.IsVisible) {
                return;
            }
            while (!IsContentLoaded) {
                await Task.Delay(100);
            }
            //show updated append buffer if not already visible
            MpNotificationBuilder.ShowNotificationAsync(MpNotificationType.AppendChanged).FireAndForgetSafeAsync();

        }
        #endregion AppendData Property
        #endregion

        #region Constructors
        public MpAvCefNetAppenderWebView() : base() {
            this.GetObservable(MpAvCefNetAppenderWebView.AppendDataProperty).Subscribe(value => OnAppendDataChanged());
            this.GetObservable(MpAvCefNetAppenderWebView.AppendModeStateProperty).Subscribe(value => OnAppendModeStateChanged());
        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
