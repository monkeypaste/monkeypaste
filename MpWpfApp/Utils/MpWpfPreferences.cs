using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MpWpfApp {
    public class MpWpfPreferences : MonkeyPaste.MpIPreferenceIO {

        #region Public Methods
        public MpWpfPreferences() {
            if (Properties.Settings.Default.DoFindBrowserUrlForCopy) {
                Properties.Settings.Default.UserDefaultBrowserProcessPath = MpHelpers.Instance.GetSystemDefaultBrowserProcessPath();
            }
            Properties.Settings.Default.UserCultureInfoName = CultureInfo.CurrentCulture.Name;
        }
        #endregion

        #region MpIPreferenceIo Implementation

        public bool Get(string key, bool defValue) {
            object pref = Properties.Settings.Default[key];
            if(pref == null) {
                return defValue;
            }
            return (bool)pref;
        }

        public int Get(string key, int defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (int)pref;
        }

        public double Get(string key, double defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (double)pref;
        }

        public float Get(string key, float defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (float)pref;
        }

        public DateTime Get(string key, DateTime defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (DateTime)pref;
        }

        public long Get(string key, long defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (long)pref;
        }

        public string Get(string key, string defValue) {
            object pref = Properties.Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (string)pref;
        }

        public Int32[] Get(string key, Int32[] defValue) {
            var sb = new StringBuilder();
            foreach (var val in (Int32[])defValue) {
                sb.AppendLine(val.ToString());
            }
            var intStr = Preferences.Get(key, sb.ToString());
            var intList = new List<Int32>();
            foreach (var val in intStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                intList.Add(Convert.ToInt32(val));
            }
            return intList.ToArray();
        }

        public MonkeyPaste.MpUserDeviceType GetDeviceType() {
            return MonkeyPaste.MpUserDeviceType.Windows;
        }

        public void Set(string key, object newValue) {
            Properties.Settings.Default[key] = newValue;

            Properties.Settings.Default.Save();
        }

        #endregion

    }
}
