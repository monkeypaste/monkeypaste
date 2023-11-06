using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Intent = Android.Content.Intent;

namespace MonkeyPaste.Avalonia.Android {
    [Activity]
    public class ProcessTextActivity : AppCompatActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            string text = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);

            Mp.Services.ContentBuilder.BuildFromDataObjectAsync(
                    new MpAvDataObject(MpPortableDataFormats.Text, text), false)
                .FireAndForgetSafeAsync();
        }

    }
}
