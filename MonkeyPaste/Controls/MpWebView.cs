using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpWebView : WebView {
        public static BindableProperty EvaluateJavascriptProperty =
            BindableProperty.Create(
                nameof(EvaluateJavascript), 
                typeof(Func<string, Task<string>>), 
                typeof(MpWebView), 
                null, BindingMode.OneWayToSource);

        public Func<string, Task<string>> EvaluateJavascript {
            get { 
                return (Func<string, Task<string>>)GetValue(EvaluateJavascriptProperty); 
            }
            set { 
                SetValue(EvaluateJavascriptProperty, value); 
            }
        }
    }
}
