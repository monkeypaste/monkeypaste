using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvExtensions {
        public static MpPoint ToPortablePoint(this Point p) {
            return new MpPoint(p.X, p.Y);
        }

        public static Point ToAvPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

    }
}
