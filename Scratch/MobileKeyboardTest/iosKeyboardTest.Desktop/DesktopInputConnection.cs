using Avalonia.Controls;
using System;

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
}
