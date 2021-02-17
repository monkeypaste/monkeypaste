using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpPasteToAppPathViewModel : MpUndoableViewModelBase<MpPasteToAppPathViewModel>, IDisposable {
        #region Private Variables

        #endregion

        #region Properties

        #region View Properties

        public BitmapSource AppIcon {
            get {
                if(PasteToAppPath == null) {
                    return new BitmapImage();
                }
                return MpHelpers.Instance.GetIconImage(AppPath);
            }
        }

        public Brush PasteToAppPathDataRowBorderBrush {
            get {
                if(IsValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        } 

        private bool _isValid = false;
        public bool IsValid {
            get {
                return _isValid;
            }
            set {
                if(_isValid != value) {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                    OnPropertyChanged(nameof(PasteToAppPathDataRowBorderBrush));
                }
            }
        }
        
        #endregion
        #region Model Properties
        public bool IsAdmin {
            get {
                if (PasteToAppPath == null) {
                    return false;
                }
                return PasteToAppPath.IsAdmin;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.IsAdmin != value) {
                    PasteToAppPath.IsAdmin = value;
                    PasteToAppPath.WriteToDatabase();
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        public string AppName {
            get {
                if (PasteToAppPath == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(PasteToAppPath.AppName)) {
                    return Path.GetFileName(PasteToAppPath.AppPath) + (IsAdmin ? " (Admin)":string.Empty);
                }
                return PasteToAppPath.AppName + (IsAdmin ? " (Admin)" : string.Empty);
            }
            set {
                if(PasteToAppPath.AppName != value && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppName = value;
                    PasteToAppPath.WriteToDatabase();
                    OnPropertyChanged(nameof(AppName));
                }
            }
        } 

        public string AppPath {
            get {
                if (PasteToAppPath == null) {
                    return String.Empty;
                }
                return PasteToAppPath.AppPath;
            }
            set {
                if (PasteToAppPath != null && PasteToAppPath.AppPath != value) {
                    PasteToAppPath.AppPath = value;
                    PasteToAppPath.WriteToDatabase();
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        public int PasteToAppPathId {
            get {
                if(PasteToAppPath == null) {
                    return 0;
                }
                return PasteToAppPath.PasteToAppPathId;
            }
            set {
                if(PasteToAppPath != null && PasteToAppPath.PasteToAppPathId != value) {
                    PasteToAppPath.PasteToAppPathId = value;
                    OnPropertyChanged(nameof(PasteToAppPathId));
                }
            }
        }

        private MpPasteToAppPath _pasteToAppPath;
        public MpPasteToAppPath PasteToAppPath {
            get {
                return _pasteToAppPath;
            }
            set {
                if(_pasteToAppPath != value) {
                    _pasteToAppPath = value;
                    OnPropertyChanged(nameof(PasteToAppPath));
                    OnPropertyChanged(nameof(PasteToAppPathId));
                    OnPropertyChanged(nameof(AppPath));
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(AppName));
                    OnPropertyChanged(nameof(AppIcon)); 
                }
            }
        }
        #endregion
        #endregion

        #region Public Methods
        public MpPasteToAppPathViewModel() : this(null) { }

        public MpPasteToAppPathViewModel(MpPasteToAppPath pasteToAppPath) {
            PasteToAppPath = pasteToAppPath;
        }

        public void Dispose() {
            PasteToAppPath.DeleteFromDatabase();
        }
        #endregion

        #region Commands

        #endregion
    }
}
