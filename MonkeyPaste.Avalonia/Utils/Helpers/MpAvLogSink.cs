using Avalonia.Logging;
#if SUGAR_WV
#endif
using MonkeyPaste.Common;
using System.Linq;

#if ENABLE_XAML_HOT_RELOAD
using HotAvalonia; 
#endif

namespace MonkeyPaste.Avalonia {
    internal class MpAvLogSink : ILogSink {
        private ILogSink _defSink;

        private (LogEventLevel, string)[] _disabledLogs = {
            (LogEventLevel.Warning,LogArea.Binding)
        };
        public static void Init() {
            _ = new MpAvLogSink();
        }
        private MpAvLogSink() {
            if (Logger.Sink != this) {
                _defSink = Logger.Sink;
            }

            Logger.Sink = this;
        }

        public bool IsEnabled(LogEventLevel level, string area) {
            if (!_defSink.IsEnabled(level, area)) {
                return false;
            }
            return _disabledLogs.All(x => x.Item1 != level && x.Item2 != area);
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate) {
            _defSink.Log(level, area, source, messageTemplate);
        }

        public void Log(LogEventLevel level, string area, object source, string messageTemplate, params object[] propertyValues) {
            _defSink.Log(level, area, source, messageTemplate, propertyValues);
        }
    }
}
