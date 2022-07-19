using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentView {
        public MpAvIContentView Default { get; }

        public MpAvContentView() {
            if(OperatingSystem.IsLinux()) {
                Default = new MpAvX11ContentView();
            } else {
                Default = new MpAvWebViewContentView();
            }
        }
    }
}
