using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdProcessWatcher : MpAvProcessWatcherBase {
        private MpPortableProcessInfo _activeProcess;
        public override IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos { get; }

        public override nint SetActiveProcess(nint handle) {
            return handle;
        }

        protected override nint GetParentHandleAtPoint(MpPoint ponint) {
            return nint.Zero;
        }

        protected override nint SetActiveProcess(nint handle, ProcessWindowStyle windowStyle) {
            throw new System.NotImplementedException();
        }

        protected override bool IsAdmin(object handleIdOrTitle) {
            return false;
        }

        protected override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            throw new System.NotImplementedException();
        }

        protected override string GetProcessPath(nint handle) {
            throw new System.NotImplementedException();
        }

        protected override nint GetActiveProcessHandle() {
            throw new System.NotImplementedException();
        }

        protected override bool IsHandleWindowProcess(nint handle) {
            throw new System.NotImplementedException();
        }

        protected override MpPortableProcessInfo GetProcessInfoByHandle(nint handle) {
            throw new System.NotImplementedException();
        }

        public MpAvAdProcessWatcher() {
            _activeProcess = new MpPortableProcessInfo();
        }

    }
}
