using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste {
    public class MpNativeWrapper {// : MpINativeInterfaceWrapper {
        #region Singleton
        private static readonly Lazy<MpNativeWrapper> _Lazy = new Lazy<MpNativeWrapper>(() => new MpNativeWrapper());
        public static MpNativeWrapper Instance { get { return _Lazy.Value; } }

        private MpNativeWrapper() { }

        #endregion

        private Dictionary<Type, object> _services { get; set; } = new Dictionary<Type, object>();

        public void Register<T>(object so) where T: class {
            //var so = Activator.CreateInstance(typeof(T));
            if(typeof(T) != so.GetType()) {
                so = so as T;
            }
            _services.Add(so.GetType(), so);
        }

        public T Get<T>() where T : class {
            return _services.Where(x => x.GetType() == typeof(T)).FirstOrDefault() as T;
        }


    }
}
