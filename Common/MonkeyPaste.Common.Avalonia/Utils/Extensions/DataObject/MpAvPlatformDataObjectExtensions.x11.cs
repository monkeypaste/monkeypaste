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

namespace MonkeyPaste.Common.Avalonia
{
    public static partial class MpAvPlatformDataObjectExtensions
    {
        public static async Task FinalizePlatformDataObjectAsync(this IDataObject ido)
        {
        }
    }
}