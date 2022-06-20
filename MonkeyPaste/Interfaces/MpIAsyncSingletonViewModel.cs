using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISingleton<T> where T : class {
        //static T Instance { get; }
        Task Init();
    }
    public interface MpISingletonViewModel<T> where T : class {
        //static T Instance { get; }
        void Init();
    }

    public interface MpIAsyncSingletonViewModel<T> where T:class {
        //static T Instance { get; }
        Task Init();
    }

    public interface MpIGlobalDependency<T> where T:class {
        void Register(T dependencyType);
    }
}
