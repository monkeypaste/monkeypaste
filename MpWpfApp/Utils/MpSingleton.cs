using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpSingleton<T> where T: new() {
        #region Singleton Definition
        private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return _Lazy.Value; } }
        #endregion
    }

    public abstract class MpSingletonViewModel<T> : MpViewModelBase<object> where T : MpViewModelBase, new() {

        #region Singleton Definition
        private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return _Lazy.Value; } }
        #endregion

        protected MpSingletonViewModel() : base(null) { }
    }


    public abstract class MpThreadSafeSingletonViewModel<T> : MpViewModelBase<object> where T : new() {

        #region Singleton Definition
        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T(),System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        //public static T Instance { get { return _Lazy.Value; } }

        public static T Instance {
            get {
                if (_instance.IsValueCreated) {
                    return _instance.Value;
                }
                lock (ThreadLock) {
                    //if (abc == null) {
                    //    abc = "Connection stored in this variable";
                    //    Console.WriteLine("Connection Made successfully");
                    //}
                }
                return _instance.Value;
            }
        }
        #endregion


        private static readonly object ThreadLock = new object();

        protected MpThreadSafeSingletonViewModel() : base(null) { }
    }
}
