using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpRectViewModel : MpViewModelBase {
        #region Properties

        #region State

        public double Left => X;
        public double Right => X + Width;
        public double Top => Y;
        public double Bottom => Y + Height;

        #endregion

        #region Model

        public double X {
            get {
                if (Location == null) {
                    return 0;
                }
                return Location.X;
            }
            set {
                if (X != value) {
                    Location.X = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y {
            get {
                if (Location == null) {
                    return 0;
                }
                return Location.Y;
            }
            set {
                if (Y != value) {
                    Location.Y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

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

        MpPoint Location {
            get {
                if(Rect == null) {
                    return null;
                }
                return Rect.Location;
            }
            set {
                if(Location != value) {
                    Rect.Location = value;
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        MpSize Size {
            get {
                if (Rect == null) {
                    return null;
                }
                return Rect.Size;
            }
            set {
                if (Size != value) {
                    Rect.Size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public MpRect Rect { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpRectViewModel() :base(null) {

        }

        public MpRectViewModel(MpRect rect) : this() {
            Rect = rect;
        }

        
        public async Task InitializeAsync(MpRect rect) {
            IsBusy = true;
            Rect = rect;
            await Task.Delay(1);
            IsBusy = false;
        }

        #endregion
    }

    //public class MpRectViewModel<T,P> : MpRectViewModel 
    //    where P:MpViewModelBase 
    //    where T:MpViewModelBase<P> {

    //    public MpRectViewModel(P p,MpIRectViewModel irect) {

    //        Rect = new MpRect() {
    //            Location = new MpPoint(irect.X, irect.Y),
    //            Size = new MpSize(irect.Width, irect.Height)
    //        };
    //    }
    //}
}
