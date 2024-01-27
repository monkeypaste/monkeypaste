using Microsoft.Win32;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public static class MpThisAppProtocolTools {
        public const string PROTOCOL_NAME = "mp";
        static MpNamedPipe<string> Pipe { get; set; }
        public static void Init() {
            // NOTE omitting this and single-instance mutex until registry editing can be figured out

            //if (!CreateProtocol()) {
            //    return;
            //}
            //Pipe = new MpNamedPipe<string>(MpNamedPipeTypes.SourceRef);
            //Pipe.OnRequest += Pipe_OnRequest;
        }

        private static void Pipe_OnRequest(string t) {
            MpConsole.WriteLine($"mp protocol requested '{t}'");
            var test = Mp.Services.SourceRefTools.FetchOrCreateSourceAsync(t);
            MpDebug.BreakAll();
        }

        static bool CreateProtocol() {
#if WINDOWS
            //HKEY_CURRENT_USER\Software\Classes
            RegistryKey parent_key = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true);
            RegistryKey key = parent_key.OpenSubKey(PROTOCOL_NAME, true);
            if (key == null) {
                // create protocol
                key = parent_key.CreateSubKey(PROTOCOL_NAME, true);
                key.SetValue(string.Empty, $"URL: {PROTOCOL_NAME}");
                key.SetValue("URL Protocol", string.Empty);

                key = key.CreateSubKey(@"shell\open\command");
                key.SetValue(string.Empty, Mp.Services.PlatformInfo.ExecutingPath + " " + "%1");
                key.Close();
            }
            return true;
#else
            return false;
#endif
        }
    }
}
