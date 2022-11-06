using Avalonia.Controls;
using Avalonia.Input;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpIFocusableViewModel : MpIViewModel {
        bool IsFocus { get; set; }
        int Level { get; }
        MpIFocusableViewModel Parent { get; }
        MpIFocusableViewModel Next { get; }
        MpIFocusableViewModel Previous { get; }
        MpIFocusableViewModel FirstChild { get; }
    }

    public class MpAvFocusManager : FocusManager {
        private static Type[] _inputControlTypes { get; set; } = new Type[] {
            typeof(TextBox),
            typeof(AutoCompleteBox),
            typeof(WebView)
        };
        public static bool IsInputControlFocused {
            get {
                IInputElement cur_focus = FocusManager.Instance.Current;
                if(cur_focus == null) {
                    return false;
                }
                bool is_input_control = _inputControlTypes.Any(x => cur_focus.GetType() == x || cur_focus.GetType().IsSubclassOf(x));
                MpConsole.WriteLine($"Current Focus Control Type: {cur_focus.GetType()} Is Input Control: {is_input_control.ToString().ToUpper()}");
                return is_input_control;

            }
        }
    }
}
