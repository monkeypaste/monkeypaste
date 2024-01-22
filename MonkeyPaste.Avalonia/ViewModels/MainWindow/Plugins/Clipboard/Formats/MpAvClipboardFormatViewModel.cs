using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIClipboardFormatMenuItemViewModel : MpAvIMenuItemViewModel {
        string Format { get; set; }
    }
    public interface MpAvIClipboardReaderFormatMenuItemViewModel : MpAvIClipboardFormatMenuItemViewModel {
    }
    public interface MpAvIClipboardWriterFormatMenuItemViewModel : MpAvIClipboardFormatMenuItemViewModel {
    }

    public class MpAvClipboardFormatViewModel :
        MpAvTreeSelectorViewModelBase<MpAvClipboardHandlerCollectionViewModel, MpAvClipboardHandlerItemViewModel>,
        MpITreeItemViewModel,
        MpISelectableViewModel {

        #region Interfaces

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvClipboardHandlerItemViewModel> AllHandlers {
            get {
                if (Parent == null) {
                    return new ObservableCollection<MpAvClipboardHandlerItemViewModel>();
                }
                return Parent.Items.Where(x => x.Items.Any(y => y.HandledFormat == FormatName));
            }
        }

        public IEnumerable<MpAvClipboardHandlerItemViewModel> Readers =>
            AllHandlers.Where(x => x.Items.Any(y => y.IsReader));

        public IEnumerable<MpAvClipboardHandlerItemViewModel> Writers =>
            AllHandlers.Where(x => x.Items.Any(y => y.IsWriter));

        #endregion


        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpAvTreeSelectorViewModelBase Overrides

        public override MpITreeItemViewModel ParentTreeItem => Parent;
        public override IEnumerable<MpITreeItemViewModel> Children {
            get {
                var c = new ObservableCollection<MpITreeItemViewModel>();
                foreach (var r in Readers) {
                    c.Add(r);
                }
                foreach (var w in Writers) {
                    c.Add(w);
                }
                return c;
            }
        }
        #endregion

        #region Appearance

        public object IconResourceObj {
            get {
                // find first enabled handler
                if (AllHandlers.FirstOrDefault(x => x.Items.Any(y => y.Items.Any(z => z.IsEnabled))) is { } hvm) {
                    // find that handlers format vm
                    if (hvm.Items.FirstOrDefault(x => x.HandledFormat == FormatName) is { } hfvm) {
                        return hfvm.HandledFormatIconId;
                    }
                }
                return "QuestionMarkImage";
            }
        }
        #endregion

        #region State

        public bool IsReadExpanded { get; set; }
        public bool IsWriteExpanded { get; set; }
        #endregion

        #region Model

        public string FormatName { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvClipboardFormatViewModel() : base(null) { }

        public MpAvClipboardFormatViewModel(MpAvClipboardHandlerCollectionViewModel parent, string format) : base(parent) {
            PropertyChanged += MpClipboardFormatViewModel_PropertyChanged;
            FormatName = format;
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
