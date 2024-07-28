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

        public void Layout(bool invalidate) {
            foreach (var kv in KeyViews) {
                kv.Layout(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Measure(bool invalidate) {
            Frame = DC.KeyboardRect.ToRectF();

            foreach (var kv in KeyViews) {
                kv.Measure(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            SetBackgroundColor(KeyboardPalette.BgHex.ToColor());

            foreach (var kv in KeyViews) {
                kv.Paint(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);

            foreach (var kv in KeyViews) {
                kv.Render(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }
    }
}