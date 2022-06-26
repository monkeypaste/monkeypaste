using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISingleton<T> where T: class {
        void Init();
    }
    public interface MpIAsyncSingleton<T> where T : class {
        //static T Instance { get; }
        Task InitAsync();
    }
    public interface MpISingletonViewModel<T> where T : class {
        //static T Instance { get; }
        void Init();
    }

    public interface MpIAsyncSingletonViewModel<T> where T:class {
        //static T Instance { get; }
        Task InitAsync();
    }

}
