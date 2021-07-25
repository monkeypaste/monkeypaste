using MonkeyPaste.iOS;
using MonkeyPaste;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(Entry), typeof(MpCustomEntryRenderer))]
namespace MonkeyPaste.iOS {
    public class MpCustomEntryRenderer : EntryRenderer {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e) {
            base.OnElementChanged(e);

            if (Control != null) {

                Control.BorderStyle = UITextBorderStyle.None;
                Control.Layer.CornerRadius = 10;
            }
        }
    }
}