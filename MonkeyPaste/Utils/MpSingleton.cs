using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpSingleton<T> where T : new() {
        #region Singleton Definition
        private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
        public static T Instance { get { return _Lazy.Value; } }
        #endregion
    }

    //public abstract class MpSingletonViewModel<T> : MpViewModelBase<object>
    //    where T : new() {

    //    #region Singleton Definition
    //    private static readonly Lazy<T> _Lazy = new Lazy<T>(() => new T());
    //    public static T Instance { get { return _Lazy.Value; } }
    //    #endregion

    //    protected MpSingletonViewModel() : base(null) { }
    //}
}
