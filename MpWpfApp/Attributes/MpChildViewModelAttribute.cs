using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    [AttributeUsage(AttributeTargets.Property)]
    public class MpChildViewModelAttribute : Attribute {
        private bool _isCollection;
        private Type _childType;

        public MpChildViewModelAttribute(Type childType, bool isCollection) {
            _isCollection = isCollection;
            _childType = childType;
        }

        public virtual bool IsCollection => _isCollection;

        public virtual Type ChildType => _childType;

    }
}
