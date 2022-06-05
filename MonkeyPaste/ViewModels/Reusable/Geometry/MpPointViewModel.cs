using System.Threading.Tasks;
using MonkeyPaste.Common;
namespace MonkeyPaste {
    public class MpPointViewModel : MpViewModelBase {
        #region Properties

        #region Model

        public double X {
            get {
                if(Point == null) {
                    return 0;
                }
                return Point.X;
            }
            set {
                if(X != value) {
                    Point.X = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y {
            get {
                if (Point == null) {
                    return 0;
                }
                return Point.Y;
            }
            set {
                if (Y != value) {
                    Point.Y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        MpPoint Point { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpPointViewModel() : base(null) { }

        public MpPointViewModel(MpPoint p) : this() {
            Point = Point;
        }

        #endregion
    }
}
