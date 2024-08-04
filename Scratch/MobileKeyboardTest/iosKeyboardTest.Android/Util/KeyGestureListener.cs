using Android.Content;
using Android.Views;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Android.Views.View;

namespace iosKeyboardTest.Android {
    public class KeyGestureListener : GestureDetector.SimpleOnGestureListener {
        Dictionary<MotionEvent, View> ViewMotionLookup { get; set; } = [];
        GestureDetector GestureDetector { get; set; }

        public KeyGestureListener(Context context) {
            GestureDetector = new GestureDetector(context, this);
        }
        public bool OnTouchEvent(View v, MotionEvent me) {
            if(!ViewMotionLookup.ContainsKey(me)) {
                ViewMotionLookup.Add(me, v);
            }
            return GestureDetector.OnTouchEvent(me);
        }

        KeyView RemoveEvent(MotionEvent me) {
            if (!ViewMotionLookup.TryGetValue(me, out var v) ||
                v is not KeyView kv) {
                if(v != null) {
                    Debug.WriteLine($"Motion Rejected: {v}");
                }
                return null;
            }
            ViewMotionLookup.Remove(me);
            return kv;
        }

        public override bool OnSingleTapUp(MotionEvent e) {
            if (RemoveEvent(e) is not { } kv) {
                return false;
            }
            Debug.WriteLine($"Single tap: {kv.DC}");
            kv.DC.PerformTapActionCommand.Execute(null);
            return false;
        }
        public override bool OnDoubleTap(MotionEvent e) {
            if (RemoveEvent(e) is not { } kv) {
                return false;
            }
            Debug.WriteLine($"Double tap: {kv.DC}");
            kv.DC.PerformDoubleTapActionCommand.Execute(null);
            return false;
        }
    }

}