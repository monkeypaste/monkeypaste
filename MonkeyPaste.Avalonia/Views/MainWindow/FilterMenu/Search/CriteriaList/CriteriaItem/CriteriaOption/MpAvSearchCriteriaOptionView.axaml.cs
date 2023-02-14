
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpCriteriaItemOptionView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaOptionView : MpAvUserControl<MpAvSearchCriteriaOptionViewModel> {
        public MpAvSearchCriteriaOptionView() {
            InitializeComponent();
            var cocc = this.FindControl<ContentControl>("CriteriaOptionContentControl");
            cocc.TemplateApplied += Cocc_TemplateApplied;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void Cocc_TemplateApplied(object sender, global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e) {
            var cocc = this.FindControl<ContentControl>("CriteriaOptionContentControl");
            if (cocc != null) {
                var mvtb = cocc.GetVisualDescendants<TextBox>().FirstOrDefault(x => x.Name == "MatchValueTextBox");
                if (mvtb != null) {
                    mvtb.LostFocus += Mvtb_LostFocus;
                }

                var test = MpPlatform.Services.PlatformResource.GetResource("DatePickerFlyoutPresenterHighlightHeight");
                var test2 = MpPlatform.Services.PlatformResource.GetResource("DatePickerFlyoutPresenterSpacerFill");
                var test3 = MpPlatform.Services.PlatformResource.GetResource("DatePickerFlyoutPresenterHighlightHeight");
                var test4 = MpPlatform.Services.PlatformResource.GetResource("DatePickerSpacerThemeWidth");

                var mvdt = cocc.GetVisualDescendant<DatePicker>();
                if (mvdt != null) {
                    mvdt.PointerEntered += Mvdt_PointerEnter;
                }
            }
        }

        private void Mvdt_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e) {
            var dp = sender as DatePicker;
            var test = dp.GetVisualDescendants<Control>();

        }

        private void Mvtb_LostFocus(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {

        }
    }
}
