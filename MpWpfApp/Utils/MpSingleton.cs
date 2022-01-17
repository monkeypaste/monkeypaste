using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpSingleton2 {
        public object InstanceObj { get; set; }
    }
    public abstract class MpSingletonViewModel : MpViewModelBase {

        public MpSingletonViewModel() : base(null) { }
    }



    public abstract class MpSingletonViewModel<T> : MpSingletonViewModel where T:class {
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
                throw new ConstructorNotFoundException("Non-public ctor() note found.");
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

    public abstract class MpSingletonBehavior<T,B> : MpBehavior<T> 
        where B : class
        where T: FrameworkElement {
        private static readonly Lazy<B> _instance = new Lazy<B>(() => {
            // Get non-public constructors for T.
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // If we can't find the right type of construcor, throw an exception.
            if (!Array.Exists(ctors, (ci) => ci.GetParameters().Length == 0)) {
                throw new ConstructorNotFoundException("Non-public ctor() note found.");
            }

            // Get reference to default non-public constructor.
            var ctor = Array.Find(ctors, (ci) => ci.GetParameters().Length == 0);

            // Invoke constructor and return resulting object.
            return ctor.Invoke(new object[] { }) as B;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static B Instance {
            get { return _instance.Value; }
        }
    }

    public abstract class MpSingleton<T> where T : class {
        private static readonly Lazy<T> _instance = new Lazy<T>(() =>
        {
            // Get non-public constructors for T.
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // If we can't find the right type of construcor, throw an exception.
            if (!Array.Exists(ctors, (ci) => ci.GetParameters().Length == 0)) {
                
                throw new ConstructorNotFoundException("Non-public ctor() note found.");
            }

            // Get reference to default non-public constructor.
            var ctor = Array.Find(ctors, (ci) => ci.GetParameters().Length == 0);

            // Invoke constructor and return resulting object.
            return ctor.Invoke(new object[] { }) as T;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Singleton instance access property.
        /// </summary>
        public static T Instance {
            get { return _instance.Value; }
        }
    }

    public abstract class MpSingleton2<T> : MpSingleton2 where T : class {
        public static T Instance {
            get { return MpResolver.Resolve<T>(); }
        }
    }

    /// <summary>
    /// Exception thrown by Singleton<T> when derived type does not contain a non-public default constructor.
    /// </summary>
    public class ConstructorNotFoundException : Exception {
        private const string ConstructorNotFoundMessage = "Singleton<T> derived types require a non-public default constructor.";
        public ConstructorNotFoundException() : base(ConstructorNotFoundMessage) { }
        public ConstructorNotFoundException(string auxMessage) : base(String.Format("{0} - {1}", ConstructorNotFoundMessage, auxMessage)) { }
        public ConstructorNotFoundException(string auxMessage, Exception inner) : base(String.Format("{0} - {1}", ConstructorNotFoundMessage, auxMessage), inner) { }
    }
}
