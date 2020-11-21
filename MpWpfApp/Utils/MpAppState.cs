using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAppState {
        private static readonly Lazy<MpAppState> _Lazy = new Lazy<MpAppState>(() => new MpAppState());
        public static MpAppState Instance { get { return _Lazy.Value; } }

    }
}
