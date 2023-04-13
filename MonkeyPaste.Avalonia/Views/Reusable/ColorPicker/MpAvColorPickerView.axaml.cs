using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvColorPickerView : UserControl {
        #region Private Variables


        #endregion
        public MpAvColorPickerView() : this(null) {
        }
        public MpAvColorPickerView(string selHexColor) : base() {
            AvaloniaXamlLoader.Load(this);

            var cancelbtn = this.GetControl<Button>("CancelButton");
            cancelbtn.Click += Cancelbtn_Click;
            var okbtn = this.GetControl<Button>("OkButton");
            okbtn.Click += Okbtn_Click;

            var cp = this.FindControl<ColorView>("Picker");
            if (selHexColor.IsStringHexColor()) {
                selHexColor = new MpColor(selHexColor).ToHex(true);
            }
            // NOTE hiding alpha when no selected or selected isn't 4 channel
            //cp.IsAlphaEnabled = selHexColor == null ? false : selHexColor.Length == 9;
            //cp.IsAlphaVisible = cp.IsAlphaVisible;
            cp.IsAlphaEnabled = false;
            cp.IsAlphaVisible = false;
            cp.Color = string.IsNullOrEmpty(selHexColor) ?
                //get
                MpColorHelpers.GetRandomHexColor().ToPortableColor().ToHex(true).ToAvColor() :
                selHexColor.ToAvColor();
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
