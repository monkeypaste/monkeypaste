using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpDependsOnParentAttribute : MpDependsOnBase {
        public MpDependsOnParentAttribute() { }

        public MpDependsOnParentAttribute(params object[] args) : base(args) { }
    }
}
