using Avalonia;
using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWebPageView : UserControl {
        #region Address Property

        private string _Address = string.Empty;

        public static readonly DirectProperty<MpAvWebPageView, string> AddressProperty =
            AvaloniaProperty.RegisterDirect<MpAvWebPageView, string>
            (
                nameof(Address),
                o => o.Address,
                (o, v) => o.Address = v,
                string.Empty
            );

        public string Address {
            get => _Address;
            set {
                SetAndRaise(AddressProperty, ref _Address, value);
            }
        }

        #endregion
        public MpAvWebPageView() {
            InitializeComponent();
        }
    }
}
