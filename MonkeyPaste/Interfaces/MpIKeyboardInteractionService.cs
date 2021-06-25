using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace MonkeyPaste {
    public interface MpIKeyboardInteractionService {
        public event EventHandler<float> OnKeyboardHeightChanged;
    }
}
