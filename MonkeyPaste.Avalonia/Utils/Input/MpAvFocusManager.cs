using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaColorPicker;
using MonkeyPaste.Common;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public interface MpIFocusableViewModel : MpIViewModel {
        bool IsFocus { get; set; }
        int Level { get; }
        MpIFocusableViewModel Parent { get; }
        MpIFocusableViewModel Next { get; }
        MpIFocusableViewModel Previous { get; }
        MpIFocusableViewModel FirstChild { get; }
    }

    public class MpAvFocusManager : MpIFocusMonitor {
        private Type[] _inputControlTypes { get; set; } = new Type[] {
            typeof(TextBox),
            typeof(AutoCompleteBox),
            typeof(ComboBoxItem),
            //typeof(WebView),
            typeof(ColorPickerWindow)
        };

        private static MpAvFocusManager _instance;
        public static MpAvFocusManager Instance => _instance ?? (_instance = new MpAvFocusManager());

        private MpAvFocusManager() { }
        public bool IsInputControlFocused {
            get {
                IInputElement cur_focus = FocusManager.Instance.Current;
                if (cur_focus == null) {
                    return false;
                }
                if (cur_focus is Window) {
                    return false;
                }
                if (cur_focus is MpIContentView wv) {
                    if (wv.IsSubSelectable) {
                        return true;
                    }
                }
                bool is_input_control = cur_focus is Control c &&
                    _inputControlTypes.Any(x =>
                        c.GetVisualAncestors().Any(y =>
                            y.GetType() == x ||
                            y.GetType().IsSubclassOf(x)));

                MpConsole.WriteLine($"Current Focus Control Type: {cur_focus.GetType()} Is Input Control: {is_input_control.ToString().ToUpper()}");
                return is_input_control || IsSelfManagedHistoryControlFocused;

            }
        }

        public bool IsSelfManagedHistoryControlFocused {
            get {
                if (FocusManager.Instance.Current is MpIContentView wv) {
                    return wv.IsSubSelectable;
                }
                return false;
            }
        }
    }
}
