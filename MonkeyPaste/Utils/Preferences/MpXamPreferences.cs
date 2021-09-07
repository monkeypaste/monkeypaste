using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpXamPreferences : MpIPreferenceIO {
        public bool Get(string key, bool defValue) {            
            return Preferences.Get(key, (bool)defValue);
        }
        public int Get(string key, int defValue) {
            return Preferences.Get(key, (int)defValue);
        }
        public double Get(string key, double defValue) {
            return Preferences.Get(key, (double)defValue);
        }
        public float Get(string key, float defValue) {
            return Preferences.Get(key, (float)defValue);
        }
        public DateTime Get(string key, DateTime defValue) {
            return Preferences.Get(key, (DateTime)defValue);
        }
        public long Get(string key, long defValue) {
            return Preferences.Get(key, (long)defValue);
        }
        public string Get(string key, string defValue) {
            return Preferences.Get(key, (string)defValue);
        }
        public Int32[] Get(string key, Int32[] defValue) {
            var sb = new StringBuilder();
            foreach (var val in (Int32[])defValue) {
                sb.AppendLine(val.ToString());
            }
            var intStr = Preferences.Get(key, sb.ToString());
            var intList = new List<Int32>();
            foreach(var val in intStr.Split(new string[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)) {
                intList.Add(Convert.ToInt32(val));
            }
            return intList.ToArray();
        }

        public MpUserDeviceType GetDeviceType() {
            switch (Device.RuntimePlatform) {
                case Device.iOS:
                    return MpUserDeviceType.Ios;
                case Device.Android:
                    return MpUserDeviceType.Android;
                case Device.macOS:
                    return MpUserDeviceType.Mac;
                case Device.GTK:
                    return MpUserDeviceType.Linux;
                default:
                    MpConsole.WriteTraceLine($"Unknown platform: {Device.RuntimePlatform.ToString()}");
                    return MpUserDeviceType.Unknown;
            }
        }

        public void Set(string key, object newValue) {
            if(newValue.GetType() == typeof(bool)) {
                Preferences.Set(key, (bool)newValue);
            } else if (newValue.GetType() == typeof(int)) {
                Preferences.Set(key, (int)newValue);
            } else if(newValue.GetType() == typeof(double)) {
                Preferences.Set(key, (double)newValue);
            } else if (newValue.GetType() == typeof(float)) {
                Preferences.Set(key, (float)newValue);
            } else if (newValue.GetType() == typeof(DateTime)) {
                Preferences.Set(key, (DateTime)newValue);
            } else if (newValue.GetType() == typeof(long)) {
                Preferences.Set(key, (long)newValue);
            } else if (newValue.GetType() == typeof(string)) {
                Preferences.Set(key, (string)newValue);
            } else if (newValue.GetType() == typeof(Int32[])) {
                var sb = new StringBuilder();
                foreach(var val in (Int32[])newValue) {
                    sb.AppendLine(val.ToString());
                }
                Preferences.Set(key, sb.ToString());
            } else {
                throw new Exception($"Uknown property type {newValue.GetType()} for key {key}");
            }
        }
    }
}
