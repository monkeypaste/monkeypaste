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

namespace MonkeyPaste.Droid {
    [Activity(Label = "Monkey Copy", NoHistory = true)]
    [IntentFilter(
        new[] { Intent.ActionProcessText },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType =  "text/plain")]
    public class MpProcessTextActivity : Activity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            var selectedText = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);
            if (!string.IsNullOrEmpty(selectedText)) {
                Console.WriteLine(@"PROCESS_TEXT: " + selectedText.ToString());

                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("SelectedText", selectedText);
                intent.PutExtra("HostInfo", this.Referrer.Host);
                StartActivity(intent);
            }
        }
    }
}