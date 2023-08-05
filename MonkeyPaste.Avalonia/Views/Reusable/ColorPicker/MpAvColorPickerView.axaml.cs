using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvColorPickerView : MpAvUserControl<object> {
        #region Private Variables


        #endregion
        public MpAvColorPickerView() : this(null) {
        }
        public MpAvColorPickerView(string selHexColor) : base() {
            AvaloniaXamlLoader.Load(this);

            var cancelbtn = this.FindControl<Button>("CancelButton");
            cancelbtn.Click += Cancelbtn_Click;
            var okbtn = this.FindControl<Button>("OkButton");
            okbtn.Click += Okbtn_Click;

            var cp = this.FindControl<ColorView>("Picker");
            if (selHexColor.IsStringHexColor()) {
                selHexColor = new MpColor(selHexColor).ToHex(true);
            }
            cp.IsAlphaEnabled = false;
            cp.IsAlphaVisible = false;
            cp.Color = string.IsNullOrEmpty(selHexColor) ?
                //get
                MpColorHelpers.GetRandomHexColor().ToPortableColor().ToHex(true).ToAvColor() :
                selHexColor.ToAvColor();
        }

        public MpAvColorPickerView(string selHexColor, string[] palette) : this(selHexColor) {
            if (palette == null || palette.Length == 0) {
                return;
            }
            var cp = this.FindControl<ColorView>("Picker");
            cp.PaletteColors = palette.Select(x => x.ToAvColor());

            cp.IsAccentColorsVisible = false;
            cp.IsColorComponentsVisible = false;
            cp.IsColorModelVisible = false;
            cp.IsColorSpectrumSliderVisible = false;
            cp.IsColorSpectrumVisible = false;
            cp.IsComponentSliderVisible = false;
            cp.IsComponentTextInputVisible = false;
            cp.IsHexInputVisible = false;
            cp.IsColorPaletteVisible = true;
        }
        private void Okbtn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.GetVisualRoot() is MpAvWindow w) {
                var cp = this.FindControl<ColorView>("Picker");
                w.DialogResult = cp.Color.ToPortableColor().ToHex(true);
                w.Close();
            }
        }

        private void Cancelbtn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.GetVisualRoot() is MpAvWindow w) {
                w.DialogResult = null;
                w.Close(null);
            }
        }
    }
}
