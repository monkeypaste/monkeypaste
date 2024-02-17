using System;

namespace MonkeyPaste.Common {
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpAppVersionAttribute : Attribute {
        public string Value { get; set; }
        public MpAppVersionAttribute(string value) {
            Value = value;
        }
    }
}
