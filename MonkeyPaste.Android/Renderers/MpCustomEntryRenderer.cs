using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Text;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using MonkeyPaste;
using MonkeyPaste.Droid;
using Android.Content;
using System.Runtime.Remoting.Contexts;
using Android.Widget;

[assembly: ExportRenderer(typeof(MpCustomEntry), typeof(MpCustomEntryRenderer))]
namespace MonkeyPaste.Droid {
    public class MpCustomEntryRenderer : EntryRenderer {
        public MpCustomEntryRenderer(Android.Content.Context context) : base(context) { }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e) {
            base.OnElementChanged(e);

            if (Control != null) {
                GradientDrawable gd = new GradientDrawable();
                gd.SetColor(global::Android.Graphics.Color.Transparent);
                this.Control.Background = gd;
                this.Control.SetRawInputType(InputTypes.TextFlagNoSuggestions);
                Control.SetHintTextColor(ColorStateList.ValueOf(global::Android.Graphics.Color.White));
            }
        }
    }
}