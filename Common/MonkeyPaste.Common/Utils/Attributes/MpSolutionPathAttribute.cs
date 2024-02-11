using System;

namespace MonkeyPaste.Common {
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public class MpSolutionPathAttribute : Attribute {
        public string Value { get; set; }
        public MpSolutionPathAttribute(string value) {
            Value = value;
        }
    }
}
