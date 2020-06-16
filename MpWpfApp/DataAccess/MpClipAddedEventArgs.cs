using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClipAddedEventArgs : EventArgs {
        public MpClip NewClip { get; set; }
        public MpClipAddedEventArgs(MpClip newClip) {
            NewClip = newClip;
        }
    }
}
