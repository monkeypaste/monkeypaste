using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAffectsThisAttribute : MpAffectsBaseAttribute {
        public MpAffectsThisAttribute() { }

        public override int FindAndNotifyProperties(object vm, string propertyName, int affectedCount = 0) {
            throw new NotImplementedException();
        }
    }
}
