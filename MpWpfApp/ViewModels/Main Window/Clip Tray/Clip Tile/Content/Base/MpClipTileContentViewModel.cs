using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public abstract class MpClipTileContentViewModel : MpViewModelBase {
        #region Properties
        public bool IsLoading { get; set; } = false;

        private MpClipTileViewModel _parent = null;
        public MpClipTileViewModel Parent {
            get {
                return _parent;
            }
            set {
                if(_parent != value) {
                    _parent = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        private MpCopyItem _copyItem = null;
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if (_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        private double _contentHeight = 0;
        public double ContentHeight {
            get {
                return _contentHeight;
            }
            set {
                if (_contentHeight != value) {
                    _contentHeight = value;
                    OnPropertyChanged(nameof(ContentHeight));
                }
            }
        }

        private double _contentWidth = 0;
        public double ContentWidth {
            get {
                return ContentWidth;
            }
            set {
                if (_contentWidth != value) {
                    _contentWidth = value;
                    OnPropertyChanged(nameof(ContentWidth));
                }
            }
        }

        private string _plainText = string.Empty;
        public string PlainText {
            get {
                return _plainText;
            }
            set {
                if (_plainText != value) {
                    _plainText = value;
                    OnPropertyChanged(nameof(PlainText));
                }
            }
        }

        private List<string> _fileDrop;
        public List<string> FileDrop {
            get {
                return _fileDrop;
            }
            set {
                if (_fileDrop != value) {
                    _fileDrop = value;
                    OnPropertyChanged(nameof(FileDrop));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpClipTileContentViewModel(MpCopyItem copyItem, MpClipTileViewModel parent) {
            IsLoading = true;
            CopyItem = copyItem;
            Parent = parent;
            PlainText = CopyItem.GetPlainText();
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands
        private RelayCommand _speakClipCommand;
        public ICommand SpeakClipCommand {
            get {
                if (_speakClipCommand == null) {
                    _speakClipCommand = new RelayCommand(SpeakClip, CanSpeakClip);
                }
                return _speakClipCommand;
            }
        }
        private bool CanSpeakClip() {
            return CopyItem.CopyItemType != MpCopyItemType.Image && CopyItem.CopyItemType != MpCopyItemType.FileList;
        }
        private void SpeakClip() {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                speechSynthesizer.Speak(PlainText);
            }
        }

        #endregion
    }
}
