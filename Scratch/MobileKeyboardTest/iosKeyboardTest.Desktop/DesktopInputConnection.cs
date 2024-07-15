using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System;
using System.Diagnostics;
using System.Text;

namespace iosKeyboardTest.Desktop;

public class DesktopInputConnection : IKeyboardInputConnection_desktop {
    TextBox InputTextBox { get; set; }
    Control RenderSource { get; set; }
    Control PointerInputSource { get; set; }
    public void OnText(string text) {
        if(InputTextBox == null) {
            return;
        }
        InputTextBox.SelectedText = text;
    }

    public void OnDelete() {
        if (InputTextBox == null) {
            return;
        }
        int sidx = Math.Max(0, Math.Min(InputTextBox.SelectionStart, InputTextBox.SelectionEnd));
        int eidx = Math.Max(0, Math.Max(InputTextBox.SelectionStart, InputTextBox.SelectionEnd));
        int len = Math.Max(0, eidx - sidx);
        if (len == 0) {
            if(sidx == 0) {
                return;
            }
            int old_idx = InputTextBox.CaretIndex;
            InputTextBox.Text = InputTextBox.Text.Substring(0, sidx - 1) + InputTextBox.Text.Substring(eidx);
            InputTextBox.CaretIndex = Math.Max(0, old_idx - 1);
        } else {
            InputTextBox.SelectedText = string.Empty;
        }
    }

    public string GetLeadingText(int n) {
        if(InputTextBox == null) {
            return string.Empty;
        }
        string pre_text = InputTextBox.Text.Substring(0, InputTextBox.SelectionStart);
        if(n < 0) {
            n = pre_text.Length;
        }
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++) {
            int pre_text_idx = pre_text.Length - 1 - i;
            if(pre_text_idx < 0) {
                break;
            }
            sb.Insert(0,pre_text[pre_text_idx]);
        }
        return sb.ToString();
    }
    public KeyboardFlags Flags =>
        KeyboardFlags.Mobile;
    public void OnDone() {

    }

    public void SetKeyboardInputSource(TextBox textBox) {
        InputTextBox = textBox;
        InputTextBox.GetObservable(TextBox.CaretIndexProperty).Subscribe(value => { OnCursorChanged?.Invoke(this, EventArgs.Empty); });
    }

    public void OnNavigate(int dx, int dy) {
        if(InputTextBox == null) {
            return;
        }
        Debug.WriteLine($"DX: {dx} DY: {dy}");
        //InputTextBox.SelectionStart = InputTextBox.SelectionEnd;
        InputTextBox.CaretIndex += CursorControlHelper.FindCaretOffset(InputTextBox.Text, InputTextBox.CaretIndex, dx, dy);
    }

    #region IHeadlessRender Implementation

    public event EventHandler<TouchEventArgs> OnPointerChanged;
    public void SetPointerInputSource(Control sourceControl) {
        PointerInputSource = sourceControl;

        PointerInputSource.PointerPressed += (s, e) => {
            var loc = e.GetPosition(PointerInputSource);
            OnPointerChanged?.Invoke(this, new TouchEventArgs(loc,TouchEventType.Press));
        };
        PointerInputSource.PointerMoved += (s, e) => {
            if (OperatingSystem.IsWindows() &&
                !e.GetCurrentPoint(s as Visual).Properties.IsLeftButtonPressed) {
                // ignore mouse movement on desktop
                //OnPointerChanged?.Invoke(this, null);
                return;
            }
            var loc = e.GetPosition(PointerInputSource);
            OnPointerChanged?.Invoke(this, new TouchEventArgs(loc,TouchEventType.Move));
        };
        PointerInputSource.PointerReleased += (s, e) => {
            var loc = e.GetPosition(PointerInputSource);
            OnPointerChanged?.Invoke(this, new TouchEventArgs(loc,TouchEventType.Release));
        };
    }
    public void SetRenderSource(Control sourceControl) {
        RenderSource = sourceControl;        
    }

    public void OnVibrateRequest() {
    }

    public event EventHandler OnCursorChanged;

    #endregion
}
