using Avalonia.Controls;
using System;

namespace iosKeyboardTest.iOS {
    public static class CursorControlHelper {
        public static int FindCaretOffset(string text, int idx, int dx, int dy) {
            int new_idx = idx + dx;
            if(new_idx < 0) {
                if(idx == 0) {
                    return 0;
                }
                dx = -new_idx;
            }
            return dx;
        }
    }

}
