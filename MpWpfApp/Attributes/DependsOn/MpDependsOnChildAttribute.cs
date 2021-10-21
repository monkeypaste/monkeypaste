using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpDependsOnChildAttribute : MpDependsOnBase {
        public MpDependsOnChildAttribute() { }

        public MpDependsOnChildAttribute(params object[] args) : base(args) { }
}
}
