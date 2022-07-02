using Microsoft.Win32;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvPreferences : MonkeyPaste.MpIPreferenceIO {

        #region Public Methods
        public MpAvPreferences() {
            if (Settings.Default.DoFindBrowserUrlForCopy) {
                Settings.Default.UserDefaultBrowserProcessPath = GetDefaultBrowserPath();
            }
            Settings.Default.UserCultureInfoName = CultureInfo.CurrentCulture.Name;
        }
        #endregion

        #region MpIPreferenceIo Implementation

        public bool Get(string key, bool defValue) {
            //object pref = Settings.Default[key];
            //if(pref == null) {
            //    return defValue;
            //}
            //return (bool)pref;
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (bool)pref;
        }

        public int Get(string key, int defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (int)pref;
        }

        public double Get(string key, double defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (double)pref;
        }

        public float Get(string key, float defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (float)pref;
        }

        public DateTime Get(string key, DateTime defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (DateTime)pref;
        }

        public long Get(string key, long defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (long)pref;
        }

        public string Get(string key, string defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return (string)pref;
        }

        public Int32[] Get(string key, Int32[] defValue) {
            return Settings.Default[key] as int[];
        }

        public string[] Get(string key, string[] defValue) {
            object pref = Settings.Default[key];
            if (pref == null) {
                return defValue;
            }
            return pref as string[];
        }

        public MonkeyPaste.MpUserDeviceType GetDeviceType() {
            return MonkeyPaste.MpUserDeviceType.Windows;
        }

        public void Set(string key, object newValue) {
            Settings.Default[key] = newValue;

            Settings.Default.Save();
        }

        public string GetDefaultBrowserPath() {
            string name = string.Empty;
            RegistryKey regKey = null;

            try {
                var regDefault = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.htm\\UserChoice", false);
                var stringDefault = regDefault.GetValue("ProgId");

                regKey = Registry.ClassesRoot.OpenSubKey(stringDefault + "\\shell\\open\\command", false);
                name = regKey.GetValue(null).ToString().ToLower().Replace("" + (char)34, "");

                if (!name.EndsWith("exe"))
                    name = name.Substring(0, name.LastIndexOf(".exe") + 4);

            }
            catch (Exception ex) {
                name = string.Format("ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}", ex.GetType(), ex.TargetSite, this.GetType());
            }
            finally {
                if (regKey != null)
                    regKey.Close();
            }

            return name;
        }

        #endregion

    }
}
