using System.Diagnostics;
using System.Threading;


namespace MonkeyPaste.Avalonia {
    public static class MpAvDebuggerHelper {
        public static void Break() {
            MpAvShortcutCollectionViewModel.Instance.StopInputListener();
            Thread.Sleep(1000);
            Debugger.Break();
            MpAvShortcutCollectionViewModel.Instance.StartInputListener();
        }
    }
}
