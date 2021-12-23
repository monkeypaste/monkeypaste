using System;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAnalyticParameterAttribute : Attribute {

        public MpAnalyticParameterAttribute(int enumValue) {
            EnumValue = enumValue;
        }

        public int EnumValue { get; private set; }

    }
}
