using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using System;


namespace iosKeyboardTest.Android {
    [Activity(
        Label = "Preferences",
        Theme = "@style/MyTheme.NoActionBar",
        ParentActivity = typeof(MainActivity))]

    [MetaData(
        "android.support.PARENT_ACTIVITY",
        //Value = "md51c3958e33f8e72dae9076079df527ba2.MainActivity")]
        Value = "crc6492226635a5d2e0e6.MainActivity")]

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
        protected override void OnDestroy() {
            base.OnDestroy();

        }

        public override bool OnOptionsItemSelected(IMenuItem item) {
            //if (item.ItemId == Android.Resource.Id.Home) {
            //    NavUtils.NavigateUpFromSameTask(this);
            //}
            return base.OnOptionsItemSelected(item);
        }


    }
    public class MyPreferencesFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey) {
            SetPreferencesFromResource(Resource.Xml.preferences, rootKey);

            UpdateSummaries();
            PreferenceManager.GetDefaultSharedPreferences(Context).RegisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key) {
            UpdateSummaries();

            if(MyInputMethodService.PrefManager is not { } prefm ||
                prefm.DefValLookup is not { } def_vals ||
                !Enum.TryParse(key,out MyPrefKeys prefKey) ||
                !def_vals.TryGetValue(prefKey,out object def_val)) {
                return;
            }

            if(key.ToLower().StartsWith("do") &&
                def_val is bool def_bool_val &&
                sharedPreferences.GetBoolean(key,def_bool_val) is bool new_bool_val) {
                prefm.SetPrefValue(prefKey, new_bool_val);
            } else if(def_val is int def_int_val &&
                        sharedPreferences.GetInt(key, def_int_val) is int new_int_val) {
                prefm.SetPrefValue(prefKey, new_int_val);
            }
        }
        void UpdateSummaries() {
            foreach(var widget_key in Enum.GetNames(typeof(MyPrefKeys))) {
                if(widget_key.ToString().StartsWith("DO_")) {
                    continue;
                }
                if(this.FindPreference(widget_key) is SeekBarPreference w &&
                    w.Dependency is { } dep_key &&
                    this.FindPreference(dep_key) is SwitchPreferenceCompat dep_w) {
                    if(dep_w.Checked) {
                        int percent = (int)(((double)w.Value / (double)w.Max) * 100d);
                        w.Summary = $"{percent}%";
                    } else {
                        w.Summary = "Disabled";
                    }
                }
            }
        }
    }
}
