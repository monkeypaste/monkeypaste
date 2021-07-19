using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Java.Interop;
using static Android.Views.ViewTreeObserver;
using Xamarin.Essentials;
using System.Reactive.Subjects;

namespace MonkeyPaste.Droid {
    public class MpGlobalLayoutListener : Java.Lang.Object, IOnGlobalLayoutListener {
        private readonly Activity activity;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpGlobalLayoutListener" /> class.
        /// </summary>
        public MpGlobalLayoutListener() {
            this.activity = Platform.CurrentActivity;

            if (this.activity?.Window?.DecorView?.ViewTreeObserver == null) {
                throw new InvalidOperationException($"{this.GetType().FullName}.Constructor: The {nameof(this.activity)} or a follow up variable is null!");
            }

            this.activity.Window.DecorView.ViewTreeObserver.AddOnGlobalLayoutListener(this);
        }

        /// <summary>
        /// Gets a <see cref="Subject{T}" /> which notifies about changes of the keyboard height.
        /// </summary>
        //public Subject<float> KeyboardHeightChanged { get; } = new Subject<float>();

        public event EventHandler<float> KeyboardHeightChanged;
        /// <inheritdoc cref="IOnGlobalLayoutListener.OnGlobalLayout" />
        public void OnGlobalLayout() {
            if (this.KeyboardHeightChanged == null) {
                return;
            }

            var screenSize = new Point();
            this.activity.WindowManager?.DefaultDisplay?.GetRealSize(screenSize);

            var screenSizeWithoutKeyboard = new Rect();
            var rootView = this.activity.FindViewById(Android.Resource.Id.Content);

            if (rootView == null) {
                return;
            }

            rootView.GetWindowVisibleDisplayFrame(screenSizeWithoutKeyboard);

            var keyboardHeight = screenSize.Y - screenSizeWithoutKeyboard.Bottom;
            var keyboardHeightInDip = keyboardHeight / Resources.System?.DisplayMetrics?.Density ?? 1;
            var keyboardHeight2 = rootView.Height - screenSizeWithoutKeyboard.Bottom + screenSizeWithoutKeyboard.Top;
            var keyboardHeightInDip2 = keyboardHeight2 / Resources.System?.DisplayMetrics?.Density ?? 1;
            //this.KeyboardHeightChanged.OnNext(keyboardHeightInDip);
            KeyboardHeightChanged.Invoke(this, keyboardHeightInDip);
        }
    }
}