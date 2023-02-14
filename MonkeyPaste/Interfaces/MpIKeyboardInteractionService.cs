using System;

namespace MonkeyPaste {
    public interface MpIKeyboardInteractionService {
        public event EventHandler<float> OnKeyboardHeightChanged;
    }
}
