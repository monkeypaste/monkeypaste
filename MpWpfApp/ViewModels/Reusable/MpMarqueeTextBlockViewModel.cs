using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpMarqueeTextBlockViewModel : MpViewModelBase {

        #region Properties

        public Duration Duration { get; set; }

        public double FontSize { get; set; } = 10;

        public string Text { get; set; }

        public string FontFamily { get; set; } = "Arial";

        public Brush ForegroundBrush { get; set; } = Brushes.Black;

        public Brush BackgroundBrush { get; set; } = Brushes.Transparent;

        public double CanvasLeft { get; set; } = 0;

        public double ContainerWidth { get; set; }

        public bool CanMarquee { get; private set; }

        #endregion

        public MpMarqueeTextBlockViewModel() : base(null) {
            PropertyChanged += MpMarqueeTextBlockViewModel_PropertyChanged;
            Duration = TimeSpan.FromSeconds(4);
        }

        private void MpMarqueeTextBlockViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Text):
                    var ft = new FormattedText(
                                Text,
                                CultureInfo.CurrentCulture,
                                FlowDirection.LeftToRight,
                                new Typeface(FontFamily),
                                FontSize,
                                ForegroundBrush,
                                new NumberSubstitution(),
                                MpJsonPreferenceIO.Instance.ThisAppDip);

                    CanMarquee = ft.Width > ContainerWidth;
                    break;
            }
        }
    }

    
}
