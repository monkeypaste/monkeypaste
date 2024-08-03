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
    public class KeyGridView : CustomViewGroup {
        #region Interfaces

        #region IKeyboardViewRenderer

        public override void Measure(bool invalidate) {
            Frame = DC.KeyboardRect.ToRectF();
            base.Measure(invalidate);
        }

        public override void Paint(bool invalidate) {
            SetBackgroundColor(KeyboardPalette.BgHex.ToColor());
            base.Paint(invalidate);
        }
        #endregion

        #endregion
        #region Properties
        #region View Models
        public KeyboardViewModel DC { get; set; }
        #endregion

        #region Views
        public PopupWindow PopupWindow1 { get; set; }
        public PopupWindow PopupWindow2 { get; set; }
        public IEnumerable<PopupWindow> PopupWindows {
            get {
                if(PopupWindow1 != null) {
                    yield return PopupWindow1;
                }
                if(PopupWindow2 != null) {
                    yield return PopupWindow2;
                }
            }
        }
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
        public List<KeyView> PopupKeyViews { get; set; } = [];

        #endregion

        #endregion

        #region Constructors

        public KeyGridView(Context context, Paint paint, KeyboardViewModel dC) : base(context,paint) {
            DC = dC;
            PopupWindow1 = CreatePopupWindow();
            PopupWindow2 = CreatePopupWindow();
            DC.OnShowPopupKeys += DC_OnShowPopupKeys;

            AddOrResetKeys();
        }

        #endregion

        #region Public Methods

        public void AddOrResetKeys() {
            this.RemoveAllViews();
            PopupKeyViews.Clear();

            foreach (var kvm in DC.Keys) {
                var kv = new KeyView(kvm, this.Context, SharedPaint).SetDefaultProps($"Key '{kvm.CurrentChar}'");
                if(kvm.IsPopupKey) {
                    PopupKeyViews.Add(kv);
                } else {
                    this.AddView(kv);
                }
            }
        }
        #endregion

        #region Private Methods


        private void DC_OnShowPopupKeys(object sender, IEnumerable<KeyViewModel> e) {
            if(sender is not KeyViewModel anchor_kvm ||
                KeyViews.FirstOrDefault(x=>x.DC == anchor_kvm) is not { } anchor_kv ||
                PopupKeyViews.Where(x=>e.Contains(x.DC)) is not { } popup_kvl_temp ||
                popup_kvl_temp.ToList() is not { } popup_kvl ||
                popup_kvl.FirstOrDefault(x=>x.DC.Row == 0 && x.DC.Column == 0) is not { } origin_popup_kv) {
                return;
            }
            var puw = PopupWindows.FirstOrDefault(x => x.ContentView.Tag == anchor_kv);
            if(puw == default ) {
                puw = PopupWindows.FirstOrDefault(x => !x.IsShowing);
                if(puw == default) {
                    return;
                }
            }
            var puw_vg = puw.ContentView as CustomViewGroup;
            anchor_kv.DC.OnHidePopup += DC_OnHidePopup;
            // set popup contents tag to anchor kvm
            puw_vg.Tag = anchor_kv;

            foreach(KeyView popup_kv in popup_kvl) {
                // remove popup from parent
                if (popup_kv.Parent is ViewGroup vg) {
                    vg.RemoveView(popup_kv);
                }
                var orig_rect = popup_kv.Frame;
                float kv_w = orig_rect.Width();
                float kv_h = orig_rect.Height();
                float kv_l = popup_kv.DC.Column * kv_w;
                float kv_t = popup_kv.DC.Row * kv_h;
                float kv_r = kv_l + kv_w;
                float kv_b = kv_t + kv_h;
                var test = new RectF(kv_l, kv_t, kv_r, kv_b);

                float t1 = popup_kv.Frame.Left - test.Left;
                float t2 = popup_kv.Frame.Top - test.Top;
                float t3 = popup_kv.Frame.Right - test.Right;
                float t4 = popup_kv.Frame.Bottom - test.Bottom;
                //popup_kv.Frame = new RectF(kv_l, kv_t, kv_r, kv_b);

                // add popup to popup windows content view group
                puw_vg.AddView(popup_kv);
            }
            // from https://stackoverflow.com/a/33363635/105028

            //puw.Width = (int)popup_kvl.Max(x => x.Frame.Right);
            //puw.Height = (int)popup_kvl.Max(x => x.Frame.Bottom);
            //int l = (int)anchor_kv.Frame.CenterX() - (puw.Width/2);
            //int t = (int)this.Frame.Top + (int)anchor_kv.Frame.Top - puw.Height;
            //int r = l + puw.Width;
            //int b = t + puw.Height;

            //int edge_pad = 5;
            //int r_diff = r - (int)Frame.Right;
            //if(r_diff > 0) {
            //    l = l - r_diff - edge_pad;
            //    r = r - r_diff - edge_pad;
            //}
            //int l_diff = (int)Frame.Left - l;
            //if(l_diff > 0) {
            //    l = l + l_diff + edge_pad;
            //    r = r + l_diff + edge_pad;
            //}
            //puw_vg.Frame = new RectF(0, 0, puw.Width, puw.Height);            
            //puw.ShowAtLocation(this, GravityFlags.NoGravity, l, t);
            //puw.Update(l, t, puw.Width, puw.Height, true);

            var pur = anchor_kvm.PopupRect.ToRectF();
            puw_vg.Frame = pur.ToBounds();
            puw.Width = (int)puw_vg.Frame.Width();
            puw.Height = (int)puw_vg.Frame.Height();
            puw_vg.Render(true);
            puw.ShowAtLocation(this, GravityFlags.NoGravity, (int)pur.Left, (int)pur.Top);
            puw.Update((int)pur.Left, (int)pur.Top, puw.Width, puw.Height, true);
        }

        private void DC_OnHidePopup(object sender, System.EventArgs e) {
            if (sender is not KeyViewModel anchor_kvm ||
                KeyViews.FirstOrDefault(x => x.DC == anchor_kvm) is not { } anchor_kv ||
                PopupWindows.FirstOrDefault(x=>x.ContentView.Tag == anchor_kv) is not { } puw_to_hide) {
                return;
            }
            puw_to_hide.Dismiss();
        }

        PopupWindow CreatePopupWindow() {
            var pw = new PopupWindow();
            pw.Focusable = false;
            pw.ClippingEnabled = false;
            pw.ContentView = new CustomViewGroup(Context).SetDefaultProps("Popup Key Container");
            return pw;
        }

        #endregion
    }
}