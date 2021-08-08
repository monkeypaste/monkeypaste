using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {
    public class MpJsProperty<T> : MpObservableObject, IDisposable  {
        private object _lastVal = null;
        private string _parentPropertyName;
        private Func<string, Task<string>> _evaluator;
        private System.Timers.Timer UpdateTimer;
        private object[] _lastArgs;
        public static int UpdateInterval = 100;

        public string JsMethod { get; private set; }


        public MpJsProperty(string name,string js,Func<string, Task<string>> evaluator) {
            _parentPropertyName = name;
            _evaluator = evaluator;
            // js should just be the function name 
            JsMethod = js;
            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Interval = UpdateInterval;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer.Start();
        }

        public async Task<T> GetValue<T>(params object[] args) where T:class {
            _lastArgs = args;
            while (_evaluator == null) {
                await Task.Delay(100);
            }
            
            var result = await _evaluator(GetMethodString(args));
            if(string.IsNullOrEmpty(result)) {
                return null;
            }

            if(typeof(T) == typeof(int)) {
                return Convert.ToInt32(result) as T;
            }
            if (typeof(T) == typeof(double)) {
                return Convert.ToDouble(result) as T;
            }
            if (typeof(T) == typeof(DateTime)) {
                return DateTime.Parse(result) as T;
            }
            if (typeof(T) == typeof(bool)) {
                if(result.Trim() == "1" || result.Trim().ToLower() == "true") {
                    return true as T;
                }
                if (result.Trim() == "0" || result.Trim().ToLower() == "false") {
                    return false as T;
                }
            }
            if (typeof(T) == typeof(string)) {
                return result as T;
            }

            return JsonConvert.DeserializeObject<T>(result) as T;
        }

        private async void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            var result = await GetValue<object>(_lastArgs);
            if(_lastVal != result) {
                OnPropertyChanged(_parentPropertyName);
            }
        }

        private string GetMethodString(params object[] args) {
            if(args == null || args.Length == 0) {
                return string.Empty;
            }
            var sb = new StringBuilder();
            foreach (var arg in args) {
                sb.Append((arg == null ? "null" : arg.ToString()) + ",");
            }
            string methodParams = sb.ToString();
            if (args != null && args.Length > 0) {
                methodParams = methodParams.Remove(methodParams.Length - 1, 1);
            }

            return string.Format(@"{0}({1})", JsMethod, methodParams);
        }

        public void Dispose() {
            UpdateTimer?.Stop();
        }
    }
}
