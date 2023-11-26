using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

using MonoMac.AppKit;
using MonoMac.CoreText;
using MonoMac.Foundation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using IDataObject = Avalonia.Input.IDataObject;

namespace MonkeyPaste.Common.Avalonia {
    public static partial class MpAvPlatformDataObjectExtensions {
        public static void LogDataObject(this IDataObject ido) {
            var actual_formats = ido.GetAllDataFormats();
            foreach (string af in actual_formats) {
                if (!ido.TryGetData(af, out string dataStr)) {
                    continue;
                }
                object data = ido.Get(af);
                MpConsole.WriteLine($"Format Name: '{af}'", true);
                MpConsole.WriteLine($"Format Type:'{data.GetType()}'");
                MpConsole.WriteLine("Format Data:");
                MpConsole.WriteLine(dataStr);
            }
        }
        public static async Task FinalizePlatformDataObjectAsync(this IDataObject ido) {
            if (ido.TryGetData(MpPortableDataFormats.Files, out IEnumerable<string> fpl)) {
                var av_fpl = await fpl.ToAvFilesObjectAsync();
                ido.Set(MpPortableDataFormats.Files, av_fpl);
            }
            // TODO make sure html,rtf and png are bytes here
        }
    }
}