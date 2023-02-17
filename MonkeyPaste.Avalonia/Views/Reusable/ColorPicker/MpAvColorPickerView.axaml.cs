using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvColorPickerView : UserControl {
        #region Private Variables


        #endregion

        #region Statics

        #region SelectedHexColor Property

        private string _selectedHexColor = default;

        public static readonly DirectProperty<MpAvColorPickerView, string> SelectedHexColorProperty =
            AvaloniaProperty.RegisterDirect<MpAvColorPickerView, string>
            (
                nameof(SelectedHexColor),
                o => o.SelectedHexColor,
                (o, v) => o.SelectedHexColor = v
            );

        public string SelectedHexColor {
            get => _selectedHexColor;
            set {
                SetAndRaise(SelectedHexColorProperty, ref _selectedHexColor, value);
            }
        }

        #endregion 


        #endregion
        public MpAvColorPickerView() {
            InitializeComponent();

            var layoutRoot = this.GetControl<Grid>("LayoutRoot");

            // ColorPicker added from code-behind
            var colorPicker = new ColorPicker() {
                Color = Colors.Blue,
                Margin = new Thickness(0, 50, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Palette = new MaterialHalfColorPalette(),
            };
            Grid.SetColumn(colorPicker, 2);
            Grid.SetRow(colorPicker, 1);

            layoutRoot.Children.Add(colorPicker);

            var cancelbtn = this.GetControl<Button>("CancelButton");
            cancelbtn.Click += Cancelbtn_Click;
            var okbtn = this.GetControl<Button>("OkButton");
            okbtn.Click += Okbtn_Click;

        }

        private void Okbtn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var w = this.GetVisualAncestor<Window>();
            if (w != null) {
                w.Tag = SelectedHexColor;
                w.Close();
            }
        }

        private void Cancelbtn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var w = this.GetVisualAncestor<Window>();
            if (w != null) {
                w.Tag = null;
                w.Close();
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
