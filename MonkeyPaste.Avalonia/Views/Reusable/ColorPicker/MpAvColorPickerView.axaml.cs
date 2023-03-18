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
        public MpAvColorPickerView() {
            AvaloniaXamlLoader.Load(this);

            var cancelbtn = this.GetControl<Button>("CancelButton");
            cancelbtn.Click += Cancelbtn_Click;
            var okbtn = this.GetControl<Button>("OkButton");
            okbtn.Click += Okbtn_Click;
        }
        public MpAvColorPickerView(string selHexColor) : this() {
            var cp = this.FindControl<ColorView>("Picker");
            cp.Color = string.IsNullOrEmpty(selHexColor) ?
                MpColorHelpers.GetRandomHexColor().ToAvColor() :
                selHexColor.ToAvColor();
        }

        private void Okbtn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (this.GetVisualRoot() is MpAvWindow w) {
                w.DialogResult = this.FindControl<ColorView>("Picker").Color.ToPortableColor().ToHex();
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
