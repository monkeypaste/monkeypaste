using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpTemplateHyperlinkCollectionViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel>, ICloneable {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        private MpRtbItemViewModel _clipTileRichTextBoxViewModel = null;
        public MpRtbItemViewModel ClipTileRichTextBoxViewModel {
            get {
                return _clipTileRichTextBoxViewModel;
            }
            set {
                if (_clipTileRichTextBoxViewModel != value) {
                    _clipTileRichTextBoxViewModel = value;
                    OnPropertyChanged(nameof(ClipTileRichTextBoxViewModel));
                }
            }
        }

        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }

        public MpObservableCollection<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelListByDocOrder {
            get {
                var ul = new MpObservableCollection<MpTemplateHyperlinkViewModel>();
                var docOrderList = this.ToList();
                docOrderList.Sort(CompareTemplatesByDocOrder);
                foreach (var thlvm in docOrderList) {
                    bool itExists = false;
                    foreach (var unm in ul) {
                        if (unm.TemplateName == thlvm.TemplateName) {
                            itExists = true;
                        }
                    }
                    if (!itExists) {
                        ul.Add(thlvm);
                    }
                }
                return ul;
            }
        }

        public MpTemplateHyperlinkViewModel SelectedTemplateHyperlinkViewModel {
            get {
                foreach (var ttcvm in this) {
                    if (ttcvm.IsSelected) {
                        return ttcvm;
                    }
                }
                //if none selected but exist select first one
                //if (this.Count > 0) {
                //    this[0].IsSelected = true;
                //    return SelectedTemplateHyperlinkViewModel;
                //}
                return null;
            }
            set {
                if (SelectedTemplateHyperlinkViewModel != value && this.Contains(value)) {
                    foreach (var ttcvm in this) {
                        if (ttcvm != value) {
                            //clear any other selections
                            ttcvm.IsSelected = false;
                        } else {
                            ttcvm.IsSelected = true;
                        }
                    }
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion        

        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel() : base() { }

        public MpTemplateHyperlinkCollectionViewModel(MpClipTileViewModel parent,MpRtbItemViewModel rtbvm) :base() {
            CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
            };

            ClipTileViewModel = parent;
            ClipTileRichTextBoxViewModel = rtbvm;

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileViewModel.IsPastingTemplate):
                        Reset();
                        break;
                }
            };
            //templates are added in the CreateHyperlinks rtb extension
        }
        public void ClearSelection() {
            foreach(var thlvm in this) {
                thlvm.IsSelected = false;
            }
        }
        public void Reset() {
            foreach (var thlvm in this) {
                thlvm.SetTemplateText(string.Empty);
            }
            ClearSelection();
        }
        public void SelectTemplate(string templateName) {
            foreach(var thlvm in this) {
                thlvm.IsSelected = thlvm.TemplateName == templateName;
            }
        }

        public void SetTemplateText(string templateName, string templateText) {
            foreach(var thlvm in this) {
                if(thlvm.TemplateName == templateName) {
                    thlvm.SetTemplateText(templateText);
                    thlvm.IsSelected = true;
                } else {
                    thlvm.IsSelected = false;
                }
            }
        }
        #endregion

        #region Private Methods
        private int CompareTemplatesByDocOrder(MpTemplateHyperlinkViewModel a, MpTemplateHyperlinkViewModel b) {
            if (a == null) {
                if (b == null) {
                    return 0;
                }
                return -1;
            } 
            if (b == null) {
                return 1;
            }
            if (a.TemplateHyperlink.ElementStart.IsInSameDocument(b.TemplateHyperlink.ElementStart)) {
                return a.TemplateHyperlink.ElementStart.CompareTo(b.TemplateHyperlink.ElementStart);
            }
            return -1;
        }
        #endregion

        #region Commands

        #endregion

        #region Overrides
        public new void Add(MpTemplateHyperlinkViewModel thlvm) {
            if(thlvm == null) {
                return;
            }
            base.Add(thlvm);
            thlvm.CopyItemTemplate.WriteToDatabase();
        }

        public object Clone() {
            var nthlcvm = new MpTemplateHyperlinkCollectionViewModel(ClipTileViewModel,ClipTileRichTextBoxViewModel);
            foreach(var thlvm in this) {
                nthlcvm.Add((MpTemplateHyperlinkViewModel)thlvm.Clone());
            }
            return nthlcvm;
        }
        #endregion
    }
}
