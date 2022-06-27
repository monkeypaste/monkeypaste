using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpClipboardFormatViewModel : 
        MpSelectorViewModelBase<MpClipboardHandlerCollectionViewModel,MpClipboardHandlerItemViewModel>,
        MpITreeItemViewModel,
        MpISelectableViewModel {

        #region Properties

        #region View Models

        public IEnumerable<MpClipboardHandlerItemViewModel> Readers {
            get {
                if(Parent == null) {
                    return new ObservableCollection<MpClipboardHandlerItemViewModel>();
                }
                return Parent.Items.Where(x => x.Items.Any(y => y.HandledFormat == FormatName && y.Items.Any(z=>z.CanRead)));
            }
        }

        public IEnumerable<MpClipboardHandlerItemViewModel> Writers {
            get {
                if (Parent == null) {
                    return new ObservableCollection<MpClipboardHandlerItemViewModel>();
                }
                return Parent.Items.Where(x => x.Items.Any(y => y.HandledFormat == FormatName && y.Items.Any(z => z.CanWrite)));
            }
        }

        #endregion


        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpITreeItemViewModel

        public bool IsExpanded { get; set; }
        public MpITreeItemViewModel ParentTreeItem => null;
        public ObservableCollection<MpITreeItemViewModel> Children { 
            get {
                var c = new ObservableCollection<MpITreeItemViewModel>();
                foreach(var r in Readers) {
                    c.Add(r);
                }
                foreach(var w in Writers) {
                    c.Add(w);
                }
                return c;
            }
        }
        #endregion

        #region Appearance

        public string IconResourceKeyStr {
            get {
                string keyStr = Format + "FormatIcon";
                if(keyStr.IsStringResourcePath()) {
                    return keyStr;
                }
                return "QuestionMarkIcon";
            }
        }
        #endregion

        #region State

        public bool IsReadExpanded { get; set; }
        public bool IsWriteExpanded { get; set; }
        #endregion

        #region Model

        public string FormatName {
            get {
                if(Format == null) {
                    return string.Empty;
                } 
                return Format.Name;
            }
        }

        public int FormatId {
            get {
                if(Format == null) {
                    return -1;
                }
                return Format.Id;
            }
        }

        public MpPortableDataFormat Format { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpClipboardFormatViewModel() : base(null) { }

        public MpClipboardFormatViewModel(MpClipboardHandlerCollectionViewModel parent, string format) : base(parent) {
            PropertyChanged += MpClipboardFormatViewModel_PropertyChanged;
            Format = MpPortableDataFormats.GetDataFormat(format);
        }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods


        private void MpClipboardFormatViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
            }
        }

        #endregion
    }
}
