using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpTemplateHyperlinkCollectionViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel> {
        #region Private Variables
        private Dictionary<string, int> _templateHyperlinkInstanceLookUp = new Dictionary<string, int>();
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

        public List<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelList {
            get {
                var ul = new List<MpTemplateHyperlinkViewModel>();
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
                ul.Sort();
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
        private Dictionary<string, string> _templateTextLookUpDictionary = new Dictionary<string, string>();
        public Dictionary<string,string> TemplateTextLookUpDictionary {
            get {
                return _templateTextLookUpDictionary;
            }
            set {
                if(_templateTextLookUpDictionary != value) {
                    _templateTextLookUpDictionary = value;
                    OnPropertyChanged(nameof(TemplateTextLookUpDictionary));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel(MpClipTileViewModel parent) :base() {
            CollectionChanged += (s, e) => {
                TemplateTextLookUpDictionary = new Dictionary<string, string>();
                foreach (var uthlvm in UniqueTemplateHyperlinkViewModelList) {
                    TemplateTextLookUpDictionary.Add(uthlvm.TemplateName, string.Empty);
                }
            };
            ClipTileViewModel = parent;
            
            //templates are added in the CreateHyperlinks rtb extension
        }
        #endregion

        #region Private Methods


        #endregion

        #region Commands

        #endregion

        #region Overrides
        public new void Add(MpTemplateHyperlinkViewModel thlvm) {
            base.Add(thlvm);
            thlvm.CopyItemTemplate.WriteToDatabase();
            //SelectedTemplateHyperlinkViewModel = thlvm;
        }

        public new bool Contains(MpTemplateHyperlinkViewModel thlvm) {
            foreach (var vm in this) {
                if (vm.TemplateName == thlvm.TemplateName &&
                   vm.TemplateTextRange.Start.CompareTo(thlvm.TemplateTextRange.Start) == 0) {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
