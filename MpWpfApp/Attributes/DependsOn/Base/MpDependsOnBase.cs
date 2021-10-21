using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class MpDependsOnBase : Attribute {
        public string[] PropertyNames { get; set; } = new string[] { };

        public MpDependsOnBase(params object[] args) {
            PropertyNames = args.Cast<string>().ToArray();
        }
    }
}
