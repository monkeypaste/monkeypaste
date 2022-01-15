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
    public abstract class MpSingleton2<T> where T: new() {
        #region Singleton Definition
        private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return _Lazy.Value; } }
        #endregion
    }

    public abstract class MpSingletonViewModel<T> : MpViewModelBase where T:class {
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

        public MpSingletonViewModel() : base(null) {
            
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

    /// <summary>
    /// A generic abstract implementation of the Singleton design pattern (http://en.wikipedia.org/wiki/Singleton_pattern).
    /// 
    /// Derived type must contain a non-public default constructor to satisfy the rules of the Singleton Pattern.
    /// If no matching constructor is found, an exception will be thrown at run-time. I am working on a StyleCop
    /// constraint that will throw a compile-time error in the future.
    /// 
    /// Example Usage (C#):
    /// 
    ///     class MySingleton : Singleton<MySingleton>
    ///     {
    ///         private const string HelloWorldMessage = "Hello World - from MySingleton";
    ///     
    ///         public string HelloWorld { get; private set; }
    ///
    ///         // Note: *** Private Constructor ***
    ///         private MySingleton()
    ///         {
    ///             // Set default message here.
    ///             HelloWorld = HelloWorldMessage;
    ///         }
    ///     }
    /// 
    ///     class Program
    ///     {
    ///         static void Main()
    ///         {
    ///             var mySingleton = MySingleton.Instance;
    ///             Console.WriteLine(mySingleton.HelloWorld);
    ///             Console.ReadKey();
    ///         }
    ///     }
    /// </summary>
    /// <typeparam name="T">Type of derived Singleton object (i.e. class MySingletone: Singleton<MySingleton>).</typeparam>
    public abstract class MpSingleton<T> where T : class {
        /// <summary>
        /// "_instance" is the meat of the Singleton<T> base-class, as it both holds the instance
        /// pointer and the reflection based factory class used by Lazy<T> for instantiation.
        /// 
        /// Lazy<T>.ctor(Func<T> valueFactory,LazyThreadSafetyMode mode), valueFactory:
        /// 
        ///     Due to the fact Lazy<T> cannot access a singleton's (non-public) default constructor and
        ///     there is no "non-public default constructor required" constraint available for C# 
        ///     generic types, Lazy<T>'s valueFactory Lambda uses reflection to create the instance.
        ///
        /// Lazy<T>.ctor(Func<T> valueFactory,LazyThreadSafetyMode mode), mode:
        /// 
        ///     Explanation of selected mode (ExecutionAndPublication) is from MSDN.
        ///     
        ///     Locks are used to ensure that only a single thread can initialize a Lazy<T> instance 
        ///     in a thread-safe manner. If the initialization method (or the default constructor, if 
        ///     there is no initialization method) uses locks internally, deadlocks can occur. If you 
        ///     use a Lazy<T> constructor that specifies an initialization method (valueFactory parameter),
        ///     and if that initialization method throws an exception (or fails to handle an exception) the 
        ///     first time you call the Lazy<T>.Value property, then the exception is cached and thrown
        ///     again on subsequent calls to the Lazy<T>.Value property. If you use a Lazy<T> 
        ///     constructor that does not specify an initialization method, exceptions that are thrown by
        ///     the default constructor for T are not cached. In that case, a subsequent call to the 
        ///     Lazy<T>.Value property might successfully initialize the Lazy<T> instance. If the
        ///     initialization method recursively accesses the Value property of the Lazy<T> instance,
        ///     an InvalidOperationException is thrown.
        /// 
        /// </summary>
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
