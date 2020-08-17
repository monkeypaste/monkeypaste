using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipTileImageViewModel : MpClipTileContentViewModel {
        private BitmapSource _bitmap;
        public BitmapSource Bitmap {
            get {
                return _bitmap;
            }
            set {
                if (_bitmap != value) {
                    _bitmap = value;
                    OnPropertyChanged(nameof(Bitmap));
                }
            }
        }

        public MpClipTileImageViewModel(MpCopyItem copyItem, MpClipTileViewModel parent) : base(copyItem,parent) {
        }
    }
}
