using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class MpAffectsBaseAttribute : Attribute {

        public MpAffectsBaseAttribute() { }

        public abstract int FindAndNotifyProperties(object vm, string propertyName, int affectedCount = 0);
    }
}
