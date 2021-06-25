using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using UIKit;
using MonkeyPaste;

namespace MonkeyPaste.iOS {
    public class MpKeyboardInteractionService : MpIKeyboardInteractionService {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractionService" /> class.
        /// </summary>
        public MpKeyboardInteractionService() {
            UIKeyboard.Notifications.ObserveWillShow((_, uiKeyboardEventArgs) => {
                var newKeyboardHeight = (float)uiKeyboardEventArgs.FrameEnd.Height;
                OnKeyboardHeightChanged?.Invoke(this,newKeyboardHeight);
            });

            UIKeyboard.Notifications.ObserveWillHide((_, uiKeyboardEventArgs) => {
                OnKeyboardHeightChanged?.Invoke(this, 0);
            });
        }

        public event EventHandler<float> OnKeyboardHeightChanged;

        /// <inheritdoc cref="IKeyboardInteractionService.KeyboardHeightChanged" />
        //public Subject<float> KeyboardHeightChanged { get; } = new Subject<float>();
    }
}