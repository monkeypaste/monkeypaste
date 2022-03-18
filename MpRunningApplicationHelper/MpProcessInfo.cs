using System;
using System.Collections.Generic;

namespace MpProcessHelper {
    public class MpProcessInfo {
        public IntPtr Handle { get; set; } = IntPtr.Zero;
        public string ProcessPath { get; set; } = string.Empty;

        public List<string> ArgumentList { get; set; }
        public bool IsSilent { get; set; }
        public bool IsAdmin { get; set; }
        public bool CreateNoWindow { get; set; }
        public bool ShowError { get; set; } = true;
        public bool CloseOnComplete { get; set; }

        public bool UseShellExecute { get; set; }
        public string WorkingDirectory { get; set; }

        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string StandardOutput { get; set; } = string.Empty;
        public string StandardError { get; set; } = string.Empty;

        public WinApi.ShowWindowCommands WindowState { get; set; }
        //public DateTime LastActiveDateTime { get; set; }
    }
}
