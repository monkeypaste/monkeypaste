using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]

    [TemplatePart("PART_HorizontalScrollBar", typeof(MpAvPagingScrollBar))]
    [TemplatePart("PART_VerticalScrollBar", typeof(MpAvPagingScrollBar))]
    public class MpAvPagingScrollViewer : ScrollViewer, IStyleable {
        Type IStyleable.StyleKey => typeof(MpAvPagingScrollViewer);
    }

    [DoNotNotify]
    public class MpAvPagingScrollBar : ScrollBar, IStyleable {
        Type IStyleable.StyleKey => typeof(MpAvPagingScrollBar);
    }
}
