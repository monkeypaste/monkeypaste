using System;

namespace MonkeyPaste {
    public interface MpIDbInfo {
        string DbExtension { get; }
        string DbFileName { get; }
        string DbDir { get; }
        string DbPath { get; }
        string DbPassword { get; }
        bool HasUserDefinedPassword { get; }

        string EnterPasswordTitle { get; }
        string EnterPasswordText { get; }
        DateTime? DbCreateDateTime { get; set; }
        void SetPassword(string pwd, bool remember);
    }

}
