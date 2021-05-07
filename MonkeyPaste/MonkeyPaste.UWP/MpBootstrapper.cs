using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.UWP {
    public class MpBootstrapper : MonkeyPaste.MpBootstrapper {
        public static void Init() {
            var instance = new MpBootstrapper();
        }
        
    }
}
