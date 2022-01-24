using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {

    public interface MpIUserIconViewModel {
        bool IsReadOnly { get; }
        Task<MpIcon> GetIcon();
        ICommand SetIconCommand { get; } //has MpIcon as path arg
    }
}
