using System;

namespace MonkeyPaste.UWP {
    public class MpKeyboardInteractionService : MpIKeyboardInteractionService {
        public event EventHandler<float> OnKeyboardHeightChanged;
    }
}