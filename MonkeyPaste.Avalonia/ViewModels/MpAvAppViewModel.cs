using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppViewModel : MpViewModelBase {
        public MpAvAppViewModel() : base() { }

        public ICommand ExitCommand => new MpCommand(
            () => {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) {
                    lifetime.Shutdown();
                    
                }
            });

        public ICommand ToggleCommand => new MpCommand(
            () => {

            });
    }
}
