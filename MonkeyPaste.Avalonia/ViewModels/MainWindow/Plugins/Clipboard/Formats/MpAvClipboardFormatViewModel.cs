﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardFormatViewModel : 
        MpAvTreeSelectorViewModelBase<MpAvClipboardHandlerCollectionViewModel,MpAvClipboardHandlerItemViewModel>,
        MpITreeItemViewModel,
        MpISelectableViewModel {

        #region Properties

        #region View Models

        public IEnumerable<MpAvClipboardHandlerItemViewModel> Readers {
            get {
                if(Parent == null) {
                    return new ObservableCollection<MpAvClipboardHandlerItemViewModel>();
                }
                return Parent.Items.Where(x => x.Items.Any(y => y.HandledFormat == FormatName && y.Items.Any(z=>z.IsReader)));
            }
        }

        public IEnumerable<MpAvClipboardHandlerItemViewModel> Writers {
            get {
                if (Parent == null) {
                    return new ObservableCollection<MpAvClipboardHandlerItemViewModel>();
                }
                return Parent.Items.Where(x => x.Items.Any(y => y.HandledFormat == FormatName && y.Items.Any(z => z.IsWriter)));
            }
        }

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
                string keyStr = "FormatIcon";
                switch(FormatName) {
                    case MpPortableDataFormats.AvRtf_bytes:
                        keyStr = "Rtf" + keyStr;
                        break;
                    case MpPortableDataFormats.OemText:
                    case MpPortableDataFormats.Unicode:
                    case MpPortableDataFormats.Text:
                        keyStr = "Text" + keyStr;
                        break;
                    case MpPortableDataFormats.AvHtml_bytes:
                        keyStr = "Html" + keyStr;
                        break;
                    case MpPortableDataFormats.AvCsv:
                        keyStr = "Csv" + keyStr;
                        break;
                    case MpPortableDataFormats.AvPNG:
                        keyStr = "Bitmap" + keyStr;
                        break;
                    default:
                        keyStr = "QuestionMarkIcon";
                        break;
                }
                

                return MpPlatformWrapper.Services.PlatformResource.GetResource(keyStr) as string;
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

        public MpAvClipboardFormatViewModel() : base(null) { }

        public MpAvClipboardFormatViewModel(MpAvClipboardHandlerCollectionViewModel parent, string format) : base(parent) {
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