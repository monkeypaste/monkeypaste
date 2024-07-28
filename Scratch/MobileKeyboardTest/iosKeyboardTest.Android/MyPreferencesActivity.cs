using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Preference;


namespace iosKeyboardTest.Android {
    [Activity(
        Label = "PrefsActivity",
        Theme = "@style/MyTheme.NoActionBar",
        ParentActivity = typeof(MainActivity))]

    [MetaData(
        "android.support.PARENT_ACTIVITY", 
        Value = "md51c3958e33f8e72dae9076079df527ba2.MainActivity")]

    public class MyPreferencesActivity : AppCompatActivity {


        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.pref_layout);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content, new MyPreferencesFragment())
                .Commit();

            if (SupportActionBar != null) {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            //if (item.ItemId == Android.Resource.Id.Home) {
            //    NavUtils.NavigateUpFromSameTask(this);
            //}
            return base.OnOptionsItemSelected(item);
        }
    }
    public class MyPreferencesFragment : PreferenceFragmentCompat {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey) {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);
        }
    }
}
