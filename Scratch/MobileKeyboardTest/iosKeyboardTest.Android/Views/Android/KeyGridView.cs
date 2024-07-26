using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace iosKeyboardTest.Android {
    public class KeyGridView : CustomViewGroup, IKeyboardViewRenderer {
        public KeyboardViewModel DC { get; set; }

        public Dictionary<PopupWindow, List<KeyView>> PopupWindowLookup { get; private set; } = [];
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
            if(PopupWindowLookup.Any()) {
                // todo need to clear windows here
            }
            PopupWindowLookup.Clear();

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

            foreach(var kv in KeyViews) {
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


        public void ShowPopup(KeyView kv) {
            if(GetWindowByAnchorKey(kv) is { } puw){
                HidePopup(kv);
            }
            puw = new PopupWindow(Context);
            puw.ClippingEnabled = false;
            var puv = new CustomViewGroup(Context, SharedPaint).SetDefaultProps();
            puv.SetBackgroundColor(Color.Purple);
            var outer_frame = kv.DC.PopupRect.ToRectF();
            puv.Frame = outer_frame.ToBounds();
            System.Diagnostics.Debug.WriteLine($"Outer Frame: {puv.Frame}");
            
            PopupWindowLookup.Add(puw, new());
            foreach (var pukvm in kv.DC.PopupKeys) {
                var pukv = new KeyView(pukvm, Context, SharedPaint).SetDefaultProps();
                var orig_rect = pukvm.InnerRect.ToRectF();
                float w = orig_rect.Width();
                float h = orig_rect.Height();
                float l = pukvm.Column * w;
                float t = pukvm.Row * h;
                float r = l + w;
                float b = t + h;
                pukv.Frame = new RectF(l, t, r, b);
                System.Diagnostics.Debug.WriteLine($"PUKV: {pukv.Frame}");
                puv.AddView(pukv);
                PopupWindowLookup[puw].Add(pukv);
            }
            //var pukvl = kv.DC.PopupKeys.Select(x => new KeyView(x, Context, SharedPaint));

            puw.ContentView = puv;
            puw.ShowAtLocation(this, GravityFlags.NoGravity, (int)outer_frame.Left, (int)outer_frame.Top);

        }
        public void HidePopup(KeyView kv) {
            if (GetWindowByAnchorKey(kv) is not { } puw) {
                return;
            }
            PopupWindowLookup.Remove(puw);
            puw.Dismiss();
        }
        PopupWindow GetWindowByAnchorKey(KeyView kv) {
            if(PopupWindowLookup.FirstOrDefault(x => x.Value != null && x.Value.Any() && x.Value.FirstOrDefault().DC.PopupAnchorKey == kv.DC) is { } kvp) {
                return kvp.Key;
            }
            return null;
        }

        public void ClearPopups() {
            foreach(var kvp in PopupWindowLookup) {
                if (KeyViews.FirstOrDefault(x=>x.DC == kvp.Value.FirstOrDefault().DC.PopupAnchorKey) is { } kv) {
                    HidePopup(kv);
                }
            }
        }
    }
}