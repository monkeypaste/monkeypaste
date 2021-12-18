using System;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpKey {
        private MpWindowsKey _key = MpWindowsKey.None;

        public Key WpfKey { get; set; }
        //    get {
        //        return (Key)((int)_key);
        //    }
        //}

        public System.Windows.Forms.Keys WinFormsKey {
            get {
                return (System.Windows.Forms.Keys)((int)_key);
            }
        }
        
        public int Priority {
            get {
                switch(WpfKey) {
                    case Key.LeftCtrl:
                        return 0;
                    case Key.RightCtrl:
                        return 1;
                    case Key.LeftAlt:
                        return 2;
                    case Key.RightAlt:
                        return 3;
                    case Key.LeftShift:
                        return 4;
                    case Key.RightShift:
                        return 5;
                    default:
                        return 6;
                }
            }
        }

        public MpKey() { }

        //public MpKey(MpWindowsKey key) {
        //    _key = key;
        //}
        //public MpKey(System.Windows.Forms.Keys winformsKey) {
        //    _key = (MpWindowsKey)((int)winformsKey);
        //}
        public MpKey(Key wpfKey) {
            //_key = (MpWindowsKey)((int)wpfKey);
            WpfKey = wpfKey;
        }

        public override string ToString() {
            return Enum.GetName(typeof(Key), WpfKey);
        }

        public override bool Equals(object obj) {
            return (obj as MpKey).WpfKey == WpfKey;
        }
    }
}
