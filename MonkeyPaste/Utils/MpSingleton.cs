using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MonkeyPaste {
    public abstract class MpSingleton<T> where T : new() {
        #region Singleton Definition
        private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return _Lazy.Value; } }
        #endregion
    }

    public abstract class MpSingletonViewModel : MpViewModelBase {

        public MpSingletonViewModel() : base() { }
    }
    public abstract class MpSingletonViewModel<T> : MpSingletonViewModel where T : class {
        private static Dictionary<string, object> _instanceLookup;

        private static readonly Lazy<T> _instance = new Lazy<T>(() => {
            if (_instanceLookup == null) {
                _instanceLookup = new Dictionary<string, object>();
            }
            string typeStr = typeof(T).Name;
            if (_instanceLookup.ContainsKey(typeStr)) {
                return _instanceLookup[typeStr] as T;
            }

            // Get non-public constructors for T.
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            // If we can't find the right type of construcor, throw an exception.
            if (!Array.Exists(ctors, (ci) => ci.GetParameters().Length == 0)) {
                //throw new ConstructorNotFoundException("Non-public ctor() note found.");
            }

            // Get reference to default non-public constructor.
            var ctor = Array.Find(ctors, (ci) => ci.GetParameters().Length == 0);

            // Invoke constructor and return resulting object.
            var instance = ctor.Invoke(new object[] { }) as T;

            _instanceLookup.Add(typeStr, instance);

            return instance;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance {
            get { return _instance.Value; }
        }
    }

    public abstract class MpSingleton {
        public object InstanceObj { get; set; }
    }
    public abstract class MpSingletonViewModel : MpViewModelBase {
        public object InstanceObj { get; set; }
        public MpSingletonViewModel() : base() { }
    }

}
