using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSourceHighlightBehavior : MpHighlightBehaviorBase<MpClipTileTitleView> {
        protected override TextRange ContentRange => null;

        public override MpHighlightType HighlightType => MpHighlightType.Source;

        public override void ScrollToSelectedItem() {
            return;
        }

        public override void ApplyHighlighting() {
            if (!IsVisible) {
                HideSourceHighlight();
            } else {
                ShowSourceHighlight(SelectedIdx >= 0);
            }
        }

        public override void Reset() {
            HideSourceHighlight();
        }

        public override async Task FindHighlighting() {
            
            await Task.Delay(1);

            _matches.Clear();
            if (AssociatedObject == null) {
                return;
            }

            var civm = AssociatedObject.BindingContext;
            var qi = MpDataModelProvider.QueryInfo;
            string st = qi.SearchText;
            if(qi.FilterFlags.HasFlag(MpContentFilterType.Url) && 
               civm.UrlViewModel != null &&
               civm.UrlViewModel.UrlPath.ContainsByCaseOrRegexSetting(qi.SearchText)) {
                _matches.Add(null);
            } else if (qi.FilterFlags.HasFlag(MpContentFilterType.UrlTitle) &&
                       civm.UrlViewModel != null &&
                       civm.UrlViewModel.UrlTitle.ContainsByCaseOrRegexSetting(qi.SearchText)) {
                _matches.Add(null);
            } else if (qi.FilterFlags.HasFlag(MpContentFilterType.AppName) &&
               civm.AppViewModel.AppName.ContainsByCaseOrRegexSetting(qi.SearchText)) {
                _matches.Add(null);
            } else if (qi.FilterFlags.HasFlag(MpContentFilterType.AppPath) &&
                civm.AppViewModel.AppPath.ContainsByCaseOrRegexSetting(qi.SearchText)) {
                _matches.Add(null);
            }

            SelectedIdx = -1;
        }

        private void ShowSourceHighlight(bool isSelected) {
            double fillOpacity = 0.3;
            string pink = Brushes.Pink.ToHex();
            string yellow = Brushes.Yellow.ToHex();
            string red = Brushes.Red.ToHex();
            Brush fill = isSelected ? pink.ToWpfBrush(fillOpacity) : yellow.ToWpfBrush(fillOpacity);
            Brush stroke = isSelected ? red.ToWpfBrush() : yellow.ToWpfBrush(); 

            AssociatedObject.SourceMatchEllipse.Fill = fill;
            AssociatedObject.SourceMatchEllipse.Stroke = stroke;

            AssociatedObject.SourceMatchEllipse.Visibility = Visibility.Visible;
        }

        private void HideSourceHighlight() {
            if(AssociatedObject == null) {
                return;
            }
            AssociatedObject.SourceMatchEllipse.Visibility = Visibility.Collapsed;
        }
    }
}
