﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIBoxViewModel {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; }
        double Height { get; }
    }
}