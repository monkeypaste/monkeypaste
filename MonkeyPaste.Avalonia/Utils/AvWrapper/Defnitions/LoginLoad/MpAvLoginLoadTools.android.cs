﻿using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoginLoadTools {

        public bool IsLoadOnLoginEnabled =>
            false;

        public async Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false) {
            await Task.Delay(1);
            return;
        }
    }
}
