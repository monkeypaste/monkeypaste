using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {
        private bool _isLoginLoadEnabled = false;
        public bool IsLoadOnLoginEnabled => _isLoginLoadEnabled;

        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            await Task.Delay(1);
        }
    }
}
