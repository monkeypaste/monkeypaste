using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvICustomWrapItemViewModel : MpIViewModel {
        public double Width { get; set; }
        public double MinWidth { get; set; }
    }
}
