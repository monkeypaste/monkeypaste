using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Media;
using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvSourceHighlightBehavior : MpAvHighlightBehaviorBase<MpAvClipTileTitleView> {
        protected override MpAvITextRange ContentRange => null;

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
            string st = MpPrefViewModel.Instance.SearchByIsCaseSensitive ? qi.SearchText : qi.SearchText.ToLower();
            if(civm.UrlViewModel != null) {                
                if (qi.FilterFlags.HasFlag(MpContentFilterType.Url)) {
                    string urlPath = MpPrefViewModel.Instance.SearchByIsCaseSensitive ? civm.UrlViewModel.UrlPath : civm.UrlViewModel.UrlPath.ToLower();
                    if (urlPath.Contains(st)) {
                        _matches.Add(null);
                    }                    
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.UrlTitle)) {
                    string urlTitle = MpPrefViewModel.Instance.SearchByIsCaseSensitive ? civm.UrlViewModel.UrlTitle : civm.UrlViewModel.UrlTitle.ToLower();
                    if (urlTitle.Contains(st)) {
                        _matches.Add(null);
                    }
                }
            }

            if (civm.AppViewModel != null) {
                if (qi.FilterFlags.HasFlag(MpContentFilterType.AppName)) {
                    string appName = MpPrefViewModel.Instance.SearchByIsCaseSensitive ? civm.AppViewModel.AppName : civm.AppViewModel.AppName.ToLower();
                    if (appName.Contains(st)) {
                        _matches.Add(null);
                    }
                }
                if (qi.FilterFlags.HasFlag(MpContentFilterType.AppPath)) {
                    string appPath = MpPrefViewModel.Instance.SearchByIsCaseSensitive ? civm.AppViewModel.AppPath : civm.AppViewModel.AppPath.ToLower();
                    if (appPath.Contains(st)) {
                        _matches.Add(null);
                    }
                }
            }
            _matches = _matches.Distinct().ToList();

            SelectedIdx = -1;
        }

        private void ShowSourceHighlight(bool isSelected) {
            double fillOpacity = 0.3;
            var fill = isSelected ? Brushes.Pink : Brushes.Yellow;
            var stroke = isSelected ? Brushes.Red : Brushes.Yellow; 

            //AssociatedObject.SourceMatchEllipse.Fill = fill;
            //AssociatedObject.SourceMatchEllipse.Stroke = stroke;
            //AssociatedObject.SourceMatchEllipse.IsVisible = true;
        }

        private void HideSourceHighlight() {
            if(AssociatedObject == null) {
                return;
            }
            //AssociatedObject.SourceMatchEllipse.IsVisible = false;
        }
    }
}
