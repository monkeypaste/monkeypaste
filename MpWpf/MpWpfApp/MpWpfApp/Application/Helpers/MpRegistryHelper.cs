using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpRegistryHelper {
        private static readonly Lazy<MpRegistryHelper> lazy = new Lazy<MpRegistryHelper>(() => new MpRegistryHelper());
        public static MpRegistryHelper Instance { get { return lazy.Value; } }

        private RegistryKey _key;

        public MpRegistryHelper() {
            //Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\MonkeyPaste");
            _key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MonkeyPaste",true);
            if(_key == null) {
                _key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MonkeyPaste");
            } /*else {
                Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\MonkeyPaste");
                _key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MonkeyPaste");
            }*/
        }
        public void SetValue(string key,object value) {            
            _key.SetValue(key,value);
        }
        public object GetValue(string key) {
            return _key.GetValue(key);
        }
    }
}
