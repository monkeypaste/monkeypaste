using System;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAnalyticParameterAttribute : Attribute {

        public MpAnalyticParameterAttribute(Enum itemEnum, int enumValue) {
            ItemEnum = itemEnum;
            EnumValue = enumValue;
        }

        public Enum ItemEnum { get; private set; }

        public int EnumValue { get; private set; }

    }
}
