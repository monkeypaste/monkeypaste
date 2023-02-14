using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.IO;

namespace MonkeyPaste.Common.Avalonia {

    public class EmbedSampleGtk : MpAvIPlatformControl {
        private Process _mplayer;

        public IPlatformHandle CreateControl(bool isSecond, IPlatformHandle parent, Func<IPlatformHandle> createDefault) {
            if (isSecond) {
                var chooser = GtkHelper.CreateGtkFileChooser(parent.Handle);
                if (chooser != null)
                    return chooser;
            }

            var control = createDefault();
            var nodes = Path.GetFullPath(Path.Combine(typeof(EmbedSampleGtk).Assembly.GetModules()[0].FullyQualifiedName,
                "..",
                "nodes.mp4"));
            _mplayer = Process.Start(new ProcessStartInfo("mplayer",
                $"-vo x11 -zoom -loop 0 -wid {control.Handle.ToInt64()} \"{nodes}\"") {
                UseShellExecute = false,

            });
            return control;
        }
    }
}


