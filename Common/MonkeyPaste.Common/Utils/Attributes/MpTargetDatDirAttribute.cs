using System;

namespace MonkeyPaste.Common {
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpTargetDatDirAttribute : Attribute {
        public string Value { get; set; }
        public MpTargetDatDirAttribute(string value) {
            Value = value;
        }
    }
}
