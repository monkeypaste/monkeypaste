namespace MonkeyPaste.Common {
    public class MpPortableStartProcessInfo : MpPortableProcessInfo {
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
    }
}
