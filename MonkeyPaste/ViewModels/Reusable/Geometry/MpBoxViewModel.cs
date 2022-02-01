using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
namespace MonkeyPaste {
    public class MpBoxViewModel : 
        MpViewModelBase,
        MpIMovableViewModel {
        #region Properties

        #region State

        public double Left => X;
        public double Right => X + Width;
        public double Top => Y;
        public double Bottom => Y + Height;

        #endregion

        #region MpIMovableViewModel Implementation

        public bool IsMoving { get; set; }

        public bool CanMove { get; set; }

        #endregion

        #region Model

        public double X {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.X;
            }
            set {
                if (X != value) {
                    Box.X = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Y;
            }
            set {
                if (Y != value) {
                    Box.Y = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double Width {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Width;
            }
            set {
                if (Width != value) {
                    Box.Width = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height {
            get {
                if (Box == null) {
                    return 0;
                }
                return Box.Height;
            }
            set {
                if (Height != value) {
                    Box.Width = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public MpBox Box { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpBoxViewModel() :base(null) {
            PropertyChanged += MpBoxViewModel_PropertyChanged;
        }


        private void MpBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        HasModelChanged = false;
                        Task.Run(async () => { await Box.WriteToDatabaseAsync(); });
                    }
                    break;
            }
        }

        public async Task InitializeAsync(MpBox box) {
            IsBusy = true;
            Box = box;
            await Task.Delay(1);
            IsBusy = false;
        }

        #endregion
    }

    //public class MpRectViewModel<T, P> : MpRectViewModel
    //    where P : MpViewModelBase
    //    where T : MpViewModelBase<P> {

    //    public MpRectViewModel(P p, MpIRectViewModel irect) {

    //        Rect = new MpRect() {
    //            Location = new MpPoint(irect.X, irect.Y),
    //            Size = new MpSize(irect.Width, irect.Height)
    //        };
    //    }
    //}
}
