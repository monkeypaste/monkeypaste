using System;

namespace MonkeyPaste.Common {
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpLocalStorageDirAttribute : Attribute {
        public string Value { get; set; }
        public MpLocalStorageDirAttribute(string value) {
            Value = value;
        }
    }
}
