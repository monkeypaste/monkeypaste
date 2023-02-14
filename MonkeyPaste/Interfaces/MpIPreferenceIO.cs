using MonkeyPaste.Common;
using System;
//using Xamarin.Essentials;

namespace MonkeyPaste {
    public interface MpIPreferenceIO {
        bool Get(string key, bool defValue);
        int Get(string key, int defValue);
        double Get(string key, double defValue);
        float Get(string key, float defValue);
        DateTime Get(string key, DateTime defValue);
        long Get(string key, long defValue);
        string Get(string key, string defValue);
        Int32[] Get(string key, Int32[] defValue);

        void Set(string key, object newValue);

        MpUserDeviceType GetDeviceType();

        string GetDefaultBrowserPath();
    }
}
