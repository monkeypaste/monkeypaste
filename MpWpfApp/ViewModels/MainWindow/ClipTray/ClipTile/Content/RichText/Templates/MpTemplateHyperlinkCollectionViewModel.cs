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

        #region View Models
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

        public MpObservableCollection<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelList {
            get {
                var ul = new MpObservableCollection<MpTemplateHyperlinkViewModel>();
                foreach(var thlvm in this) {
                    bool itExists = false;
                    foreach(var unm in ul) {
                        if(unm.TemplateName == thlvm.TemplateName) {
                            itExists = true;
                        }
                    }
                    if(!itExists) {
                        ul.Add(thlvm);
                    }
                }
                ul.Sort(x => x["TemplateName"]);
                return ul;
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
                if(this.Count > 0) {
                    this[0].IsSelected = true;
                    return SelectedTemplateHyperlinkViewModel;
                }
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

        #region Properties
        //private Dictionary<string, string> _templateTextLookUpDictionary = new Dictionary<string, string>();
        //public Dictionary<string,string> TemplateTextLookUpDictionary {
        //    get {
        //        return _templateTextLookUpDictionary;
        //    }
        //    set {
        //        if(_templateTextLookUpDictionary != value) {
        //            _templateTextLookUpDictionary = value;
        //            OnPropertyChanged(nameof(TemplateTextLookUpDictionary));
        //        }
        //    }
        //}
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel(MpClipTileViewModel parent) :base() {
            CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelList));
                OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
                //OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
            };
            ClipTileViewModel = parent;

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileViewModel.IsPastingTemplateTile):
                        Reset();
                        break;
                }
            };
            //templates are added in the CreateHyperlinks rtb extension
        }
        public void Reset() {
            foreach (var thlvm in this) {
                thlvm.SetTemplateText(string.Empty);
            }
            if(this.Count > 0) {
                this[0].IsSelected = true;
            }
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
                } else {
                    return -1;
                }
            } else {
                if (b == null) {
                    return 1;
                } else {
                    return a.TemplateHyperlink.ElementStart.CompareTo(b.TemplateHyperlink.ElementStart);
                }
            }
        }
        #endregion

        #region Commands

        #endregion

        #region Overrides
        public new void Add(MpTemplateHyperlinkViewModel thlvm) {
            base.Add(thlvm);
            thlvm.CopyItemTemplate.WriteToDatabase();

            thlvm.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(thlvm.IsSelected):
                    case nameof(thlvm.IsHovering):
                        foreach(var vm in this) {
                            if(vm.TemplateName == thlvm.TemplateName) {
                                vm.IsHovering = thlvm.IsHovering;
                                vm.IsSelected = thlvm.IsSelected;
                            } else {
                                vm.IsHovering = vm.IsSelected = false;
                            }
                            
                        }
                        break;
                }
            };
        }

        public object Clone() {
            var nthlcvm = new MpTemplateHyperlinkCollectionViewModel(ClipTileViewModel);
            foreach(var thlvm in this) {
                nthlcvm.Add((MpTemplateHyperlinkViewModel)thlvm.Clone());
            }
            return nthlcvm;
        }
        #endregion
    }
}
