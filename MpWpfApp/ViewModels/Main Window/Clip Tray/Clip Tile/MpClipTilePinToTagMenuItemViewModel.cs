using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTilePinToTagMenuItemViewModel : MpViewModelBase {
        private bool _isTagLinkedToClip = false;
        public bool IsTagLinkedToClip {
            get {
                return _isTagLinkedToClip;
            }
            set {
                if (_isTagLinkedToClip != value) {
                    _isTagLinkedToClip = value;
                    OnPropertyChanged(nameof(IsTagLinkedToClip));
                }
            }
        }

        private string _header;
        public string Header {
            get {
                return _header;
            }
            set {
                if (_header != value) {
                    _header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

        private ICommand _command;
        public ICommand Command {
            get {
                return _command;
            }
            set {
                if (_command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        private MpTagTileViewModel _tagViewModel;
        public MpTagTileViewModel TagViewModel {
            get {
                return _tagViewModel;
            }
            set {
                if (_tagViewModel != value) {
                    _tagViewModel = value;
                    OnPropertyChanged(nameof(TagViewModel));
                }
            }
        }

        public MpClipTilePinToTagMenuItemViewModel(MpTagTileViewModel tagViewModel, ICommand command, bool isTagLinkedWithClip) {
            TagViewModel = tagViewModel;
            Header = TagViewModel.TagName;
            Command = command;
            IsTagLinkedToClip = isTagLinkedWithClip;
        }
    }
}
