using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MpDependsOnParentAttribute : Attribute {
        private uint _parentLevel = 1;

        public MpDependsOnParentAttribute() { }

        public MpDependsOnParentAttribute(uint parentLevel) {
            _parentLevel = parentLevel;
        }

        public virtual uint ParentLevel => _parentLevel;
    }
}
