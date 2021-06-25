using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonkeyPaste;
using static MonkeyPaste.Droid.MpKeyboardInteractionService;
using System.Reactive.Subjects;

namespace MonkeyPaste.Droid {
    public class MpKeyboardInteractionService : MpIKeyboardInteractionService {
        public event EventHandler<float> OnKeyboardHeightChanged;

        public MpKeyboardInteractionService() {
            var globalLayoutListener = new MpGlobalLayoutListener();
            globalLayoutListener.KeyboardHeightChanged += (s, e) => {
                OnKeyboardHeightChanged?.Invoke(s, e);
            };            
        }
    }
}