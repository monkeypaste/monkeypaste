
using System;
using System.Collections.Generic;
using System.Linq;
namespace MonkeyPaste {
    public static class MpResolver {
        private static Dictionary<Type,object> _typeLookup;

        public static void Initialize() {
            _typeLookup = new Dictionary<Type, object>();
        }

        public static void Register<T>(T dependencyType) where T:class {
            _typeLookup.AddOrReplace(dependencyType.GetType(), dependencyType);
        }

        public static void Unregister<T>(T dependencyType) where T : class {
            _typeLookup.Remove(dependencyType.GetType());
        }

        public static T Resolve<T>() where T: class {
            return Resolve(typeof(T)) as T;
        }

        public static object Resolve(Type dependencyType) {
            return _typeLookup[dependencyType];
        }
    }
}

