using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace iosKeyboardTest.Desktop;

public class DesktopInputConnection : IKeyboardInputConnection_desktop {
    TextBox InputTextBox { get; set; }
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

    public void OnDone() {

    }

    public void SetInputSource(TextBox textBox) {
        InputTextBox = textBox;
    }

    public void OnNavigate(int dx, int dy) {
        if(InputTextBox == null) {
            return;
        }
        Debug.WriteLine($"DX: {dx} DY: {dy}");
        //InputTextBox.SelectionStart = InputTextBox.SelectionEnd;
        InputTextBox.CaretIndex += CursorControlHelper.FindCaretOffset(InputTextBox.Text, InputTextBox.CaretIndex, dx, dy);
    }
}
