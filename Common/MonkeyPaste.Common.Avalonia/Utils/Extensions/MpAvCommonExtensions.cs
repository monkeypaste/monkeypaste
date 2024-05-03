using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Avalonia.VisualExtensions;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvCommonExtensions {

        public static TopLevel GetMainTopLevel(this Application? app) {
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cdsal) {
                return cdsal.MainWindow;
            }
            if (app.ApplicationLifetime is ISingleViewApplicationLifetime sval &&
                TopLevel.GetTopLevel(sval.MainView) is TopLevel tl) {
                return tl;
            }
            return null;
        }
    }
}
