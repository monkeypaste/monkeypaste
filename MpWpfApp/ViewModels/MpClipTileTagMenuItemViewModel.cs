using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTileTagMenuItemViewModel : MpViewModelBase {
        private bool _isTagLinkedToClip = false;
        public bool IsTagLinkedToClip {
            get {
                return _isTagLinkedToClip;
            }
            set {
                if(_isTagLinkedToClip != value) {
                    _isTagLinkedToClip = value;
                    OnPropertyChanged("IsTagLinkedToClip");
                }
            }
        }

        private string _header;
        public string Header {
            get {
                return _header;
            }
            set {
                if(_header != value) {
                    _header = value;
                    OnPropertyChanged("Header");
                }
            }
        }

        private ICommand _command;
        public ICommand Command {
            get {
                return _command;
            }
            set {
                if(_command != value) {
                    _command = value;
                    OnPropertyChanged("Command");
                }
            }
        }
        public MpClipTileTagMenuItemViewModel(string header,ICommand command,bool isTagLinkedWithClip) {
            Header = header;
            Command = command;
            IsTagLinkedToClip = isTagLinkedWithClip;
        }
    }
}
