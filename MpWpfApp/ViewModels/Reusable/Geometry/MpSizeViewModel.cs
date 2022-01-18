using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSizeViewModel : MpViewModelBase {
        #region Properties

        #region Model

        public double Width {
            get {
                if (Size == null) {
                    return 0;
                }
                return Size.Width;
            }
            set {
                if (Width != value) {
                    Size.Width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height {
            get {
                if (Size == null) {
                    return 0;
                }
                return Size.Height;
            }
            set {
                if (Height != value) {
                    Size.Width = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        MpSize Size { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpSizeViewModel() : base(null) {
        }

        public MpSizeViewModel(MpSize s) : this() {
            Size = s;
        }

        #endregion
    }
}
