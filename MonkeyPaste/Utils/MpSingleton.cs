using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpSingleton<TSingleton,TBase> where TBase : new ()  where TSingleton : new() {
        #region Singleton
        private static readonly Lazy<TSingleton> _Lazy = new Lazy<TSingleton>(() => new TSingleton());
        public static TSingleton Instance { get { return _Lazy.Value; } }
        #endregion

        public abstract void Init();
    }
}
