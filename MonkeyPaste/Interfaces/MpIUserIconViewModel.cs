﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {

    public interface MpIUserIconViewModel : MpIViewModel {
        int IconId { get; set; }
    }
}
