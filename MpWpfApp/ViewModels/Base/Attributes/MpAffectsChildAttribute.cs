using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpAffectsChildAttribute : Attribute {
        private bool _isCollection;
        private Type _childType;

        public MpAffectsChildAttribute() { }

        public MpAffectsChildAttribute(bool isCollection, Type childType) {
            _isCollection = isCollection;
            _childType = childType;
        }

        public virtual bool IsCollection => _isCollection;

        public virtual Type ChildType => _childType;
    }
}
