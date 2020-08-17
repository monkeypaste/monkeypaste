using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClipTileFileListViewModel : MpClipTileContentViewModel {
        private ObservableCollection<MpFileListItemViewModel> _fileListItems = null;
        public ObservableCollection<MpFileListItemViewModel> FileListItems {
            get {
                if (_fileListItems == null) {
                    _fileListItems = new ObservableCollection<MpFileListItemViewModel>();
                    foreach (string fileItem in (string[])CopyItem) {
                        _fileListItems.Add(new MpFileListItemViewModel(fileItem));
                    }
                }
                return _fileListItems;
            }
            set {
                if (_fileListItems != value) {
                    _fileListItems = value;
                    OnPropertyChanged(nameof(FileListItems));
                }
            }
        }

        public MpClipTileFileListViewModel(object content, MpClipTileViewModel parent) : base(content,parent) {

        }
    }
}
