using Avalonia.Controls;
using System;

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
            typeof(MpAvColorPickerView)
        };

        private static MpAvFocusManager _instance;
        public static MpAvFocusManager Instance => _instance ?? (_instance = new MpAvFocusManager());

        private MpAvFocusManager() { }

        public bool IsTextInputControlFocused =>
            FocusElement is Control c &&
            c.IsTextInputControl();

        public bool IsSelfManagedHistoryControlFocused {
            get {
                if (FocusElement is MpIContentView wv) {
                    return wv.IsSubSelectable;
                }
                return false;
            }
        }

        public object FocusElement {
            get {
                if (MpAvWindowManager.ActiveWindow is MpAvWindow w &&
                    MpAvWindowManager.GetTopLevel(w) is TopLevel tl) {
                    var fe = tl.FocusManager.GetFocusedElement();
                    if (fe == null || MpAvWindowManager.GetTopLevel(fe as Control) != tl) {
                        // focus manager
                        // a) doesn't treat active window as a focused control
                        // b) seems to not care what top level you give it and returns focused input control
                        return tl;
                    }
                    return fe;
                }
                return null;
            }
        }
    }
}
