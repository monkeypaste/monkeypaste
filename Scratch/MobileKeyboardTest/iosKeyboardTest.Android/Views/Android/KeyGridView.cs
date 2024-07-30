using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.ApplicationModel;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Android.Views.View;

namespace iosKeyboardTest.Android {
    public class KeyGridView : CustomViewGroup, IKeyboardViewRenderer {
        public KeyboardViewModel DC { get; set; }
        public IEnumerable<KeyView> KeyViews {
            get {
                for (int i = 0; i < ChildCount; i++) {
                    if (GetChildAt(i) is not KeyView kv) {
                        continue;
                    }
                    yield return kv;
                }
            }
        }

        public KeyGridView(Context context, Paint paint, KeyboardViewModel dC) : base(context,paint) {
            DC = dC;
            AddOrResetKeys();
        }

        public void AddOrResetKeys() {
            this.RemoveAllViews();

            foreach (var kvm in DC.Keys) {
                var kv = new KeyView(kvm, this.Context, SharedPaint).SetDefaultProps($"Key '{kvm.CurrentChar}'");
                this.AddView(kv);
            }
        }


        public override void Measure(bool invalidate) {
            Frame = DC.KeyboardRect.ToRectF();
            base.Measure(invalidate);
        }

        public override void Paint(bool invalidate) {
            SetBackgroundColor(KeyboardPalette.BgHex.ToColor());
            base.Paint(invalidate);
        }
    }
}