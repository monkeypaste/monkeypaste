using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace MonkeyPaste.iOS {
    public class MpBootstrapper : MonkeyPaste.MpBootstrapper {
        public static void Init() {
            var instance = new MpBootstrapper();
        }
    }

    
}