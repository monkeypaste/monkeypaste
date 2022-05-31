using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpShape {
        public virtual MpPoint[] Points { get; set; }
        public MpShape() { }
    }
}
