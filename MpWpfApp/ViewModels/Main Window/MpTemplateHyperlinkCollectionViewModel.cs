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
                if (SelectedTemplateHyperlinkViewModel != value) {
                    foreach (var ttcvm in this) {
                        if(ttcvm != value) {
                            //clear any other selections
                            ttcvm.IsSelected = false;
                        }
                    }
                    if (value != null && this.Contains(value)) {
                        this[this.IndexOf(value)].IsSelected = true;
                    }
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel(MpClipTileViewModel parent) :base() {
            ClipTileViewModel = parent;
            
            //templates are added in the CreateHyperlinks rtb extension
        }
        #endregion

        #region Private Methods


        #endregion

        #region Commands

        #endregion

        #region Overrides
        private new void Add(MpTemplateHyperlinkViewModel thlvm) {
            //disable default collection to enforce associated text range
            base.Add(thlvm);
        }
        public new bool Contains(MpTemplateHyperlinkViewModel thlvm) {
            foreach(var vm in this) {
                if(vm.TemplateName == thlvm.TemplateName) {
                    return true;
                }
            }
            return false;
        }

        public Hyperlink Add(MpTemplateHyperlinkViewModel thlvm, TextRange tr) {
            if (!this.Contains(thlvm)) {
                base.Add(thlvm);
                if(!_templateHyperlinkInstanceLookUp.ContainsKey(thlvm.TemplateName)) {
                    _templateHyperlinkInstanceLookUp.Add(thlvm.TemplateName, 1);
                } else {
                    _templateHyperlinkInstanceLookUp[thlvm.TemplateName]++;
                }

                if (!ClipTileViewModel.CopyItem.TemplateList.Contains(thlvm.CopyItemTemplate)) {
                    ClipTileViewModel.CopyItem.TemplateList.Add(thlvm.CopyItemTemplate);
                }
            } 
            var thlb = new MpTemplateHyperlinkBorder(thlvm);
            var container = new InlineUIContainer(thlb);
            tr.Text = string.Empty;
            var hl = new Hyperlink(tr.Start, tr.End);
            hl.Inlines.Clear();
            hl.Inlines.Add(container);
            thlvm.RangeList.Add(tr);

            return hl;
        }

        public new void Remove(MpTemplateHyperlinkViewModel thlvm) {
            if(_templateHyperlinkInstanceLookUp.ContainsKey(thlvm.TemplateName)) {
                _templateHyperlinkInstanceLookUp[thlvm.TemplateName]--;
                if(_templateHyperlinkInstanceLookUp[thlvm.TemplateName] <= 0) {
                    base.Remove(thlvm);
                    _templateHyperlinkInstanceLookUp.Remove(thlvm.TemplateName);

                    if (ClipTileViewModel.CopyItem.TemplateList.Contains(thlvm.CopyItemTemplate)) {
                        ClipTileViewModel.CopyItem.TemplateList.Remove(thlvm.CopyItemTemplate);
                        thlvm.CopyItemTemplate.DeleteFromDatabase();
                    }
                }
            }
        }
        #endregion
    }
}
