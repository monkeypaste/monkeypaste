using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvDownKeyHelper {
        private GlobalKeyboardHook _globalKeyboardHook;
        private bool _unifyKeys = false;

        public MpAvDownKeyHelper(bool unifyKeys) {
            _unifyKeys = unifyKeys;
            Init();
        }

        private void Init() {
            // Hooks into all keys.
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardEvent += OnKeyEvent;
        }

        private void OnKeyEvent(object sender, GlobalKeyboardHookEventArgs e) {
            try {
                // EDT: No need to filter for VkSnapshot anymore. This now gets handled
                // through the constructor of GlobalKeyboardHook(...).
                Key avkey = e.AvaloniaKey;
                if (_unifyKeys) {
                    avkey = avkey.GetUnifiedKey();
                }
                string keystr = Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { avkey } });
                var kcl = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
                KeyCode kc = default;
                if (kcl.Any() &&
                    kcl.First().Any()) {
                    kc = kcl[0][0];
                }
                if (kc == default || kc == KeyCode.VcUndefined) {
                    // should this happen?

                    return;
                }

                //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
                //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
                //{
                //    MessageBox.Show("Alt + Print Screen");
                //    e.Handled = true;
                //}

                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown ||
                    e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown) {
                    if (!Downs_internal.Contains(kc)) {
                        Downs_internal.Add(kc);
                        MpConsole.WriteLine($"WIN DOWN '{kc}'");
                        OnDownsChanged?.Invoke(this, new(true, kc));
                    }
                    return;
                }

                if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyUp ||
                    e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyUp) {
                    bool success = Downs_internal.Remove(kc);
                    if (success) {
                        MpConsole.WriteLine($"WIN UP '{kc}'");

                        OnDownsChanged?.Invoke(this, new(false, kc));
                    }
                    return;
                }
            }
            catch {

            }
        }
    }
}
