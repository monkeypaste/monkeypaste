using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using MonkeyPaste.Common;
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
    public class MpAvPagingScrollViewer : ScrollViewer {
        //Type IStyleable.StyleKey => typeof(MpAvPagingScrollViewer);

        public IReadOnlyList<MpAvPagingScrollBar> ScrollBars { get; private set; }

        private List<Track> _tracks;
        public IReadOnlyList<Track> Tracks {
            get {
                if (_tracks == null &&
                    ScrollBars != null &&
                    ScrollBars.Where(x => x.Track != null) is IEnumerable<MpAvPagingScrollBar> psbl &&
                    psbl.Count() == 2) {
                    _tracks = psbl.Select(x => x.Track).ToList();
                }
                return _tracks;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            var hsb = e.NameScope.Find<MpAvPagingScrollBar>("PART_HorizontalScrollBar");
            var vsb = e.NameScope.Find<MpAvPagingScrollBar>("PART_VerticalScrollBar");
            MpDebug.Assert(hsb != null && vsb != null, "Need scrollbars...");
            if (hsb == null || vsb == null) {
                return;
            }
            hsb.ApplyTemplate();
            vsb.ApplyTemplate();
            ScrollBars = new[] { hsb, vsb }.ToList();

        }
    }

    [DoNotNotify]
    [TemplatePart("PART_Track", typeof(Track))]
    public class MpAvPagingScrollBar : ScrollBar {

        #region Overrides
        //protected override Type StyleKeyOverride => typeof(MpAvPagingScrollBar);
        #endregion

        public Track Track { get; private set; }
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            Track = e.NameScope.Find<Track>("PART_Track");
            MpDebug.Assert(Track != null, "Need track...");
        }
    }
}
