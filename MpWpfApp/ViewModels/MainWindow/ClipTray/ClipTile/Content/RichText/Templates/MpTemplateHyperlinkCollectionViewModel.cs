using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpTemplateHyperlinkCollectionViewModel : MpUndoableViewModelBase<MpTemplateHyperlinkViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        private MpRtbItemViewModel _hostRtbViewModel = null;
        public MpRtbItemViewModel HostRtbViewModel {
            get {
                return _hostRtbViewModel;
            }
            set {
                if (_hostRtbViewModel != value) {
                    _hostRtbViewModel = value;
                    OnPropertyChanged(nameof(HostRtbViewModel));
                }
            }
        }
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(HostRtbViewModel == null) {
                    return null;
                }
                return HostRtbViewModel.HostClipTileViewModel;
            }
        }

        private ObservableCollection<MpTemplateHyperlinkViewModel> _templates = new ObservableCollection<MpTemplateHyperlinkViewModel>();
        public ObservableCollection<MpTemplateHyperlinkViewModel> Templates {
            get {
                return _templates;
            }
            private set {
                if(_templates != value) {
                    _templates = value;
                    OnPropertyChanged(nameof(Templates));
                }
            }
        }

        public MpTemplateHyperlinkViewModel SelectedTemplate {
            get {
                return Templates.Where(x => x.IsSelected).FirstOrDefault();
            }
        }
        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates {
            get {
                return Templates.Count > 1;
            }
        }

        public string PasteButtonText {
            get {
                return IsAllTemplatesFilled ? "PASTE" : "CONTINUE";
            }
        }
        #endregion

        #region State
        public bool IsAllTemplatesFilled {
            get {
                return Templates.Any(x => x.HasText);
            }
        }
        public int SelectedTemplateIdx {
            get {
                if(SelectedTemplate == null) {
                    return 0;
                }
                return Templates.IndexOf(SelectedTemplate);
            }
            set {
                if(value >= 0 && value < Templates.Count) {
                    ClearSelection();
                    Templates[value].IsSelected = true;
                }
            }
        }

        public bool IsEditingTemplate {
            get {
                return Templates.Any(x => x.IsEditingTemplate);
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel() : base() { }

        public MpTemplateHyperlinkCollectionViewModel(MpRtbItemViewModel rtbvm) : base() {
            

            Templates.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(Templates));
            };

            HostRtbViewModel = rtbvm;

            HostClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(HostClipTileViewModel.IsPastingTemplate):
                        ResetAll();
                        break;
                }
            };
            //templates are added in the CreateHyperlinks rtb extension
        }

        public void ClearAllEditing() {
            foreach(var thlvm in Templates) {
                thlvm.IsEditingTemplate = false;
            }
        }

        public void ClearSelection() {
            foreach(var thlvm in Templates) {
                thlvm.IsSelected = false;
            }
        }

        public void ResetAll() {
            foreach (var thlvm in Templates) {
                thlvm.Reset();
            }
            ClearSelection();
        }

        public void SetTemplateText(string templateName, string templateText) {
            foreach (var thlvm in Templates) {
                if (thlvm.TemplateName == templateName) {
                    thlvm.SetTemplateText(templateText);
                    thlvm.IsSelected = true;
                } else {
                    thlvm.IsSelected = false;
                }
            }
        }

        public MpTemplateHyperlinkViewModel CreateTemplateHyperlinkViewModel(MpCopyItemTemplate ncit) {
            if(ncit == null) {
                ncit = MpCopyItemTemplate.Create(
                            HostRtbViewModel.CopyItem.Id,
                            GetUniqueTemplateName());
            }
            var ntvm = new MpTemplateHyperlinkViewModel(this, ncit);
            ntvm.OnTemplateSelected += Ntvm_OnTemplateSelected;
            return ntvm;
        }

        private void Ntvm_OnTemplateSelected(object sender, EventArgs e) {
            var sthlvm = sender as MpTemplateHyperlinkViewModel;
            foreach(var thlvm in Templates) {
                if(thlvm != sthlvm) {
                    thlvm.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(SelectedTemplate));
        }
        #endregion

        #region Private Methods

        #region Db Event Handlers

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e != null && e is MpCopyItemTemplate cit) {
                if(cit.CopyItemId == HostRtbViewModel.CopyItem.Id) {

                }
            }
        }
        #endregion

        private string GetUniqueTemplateName() {
            HostRtbViewModel.SaveToDatabase();
            int uniqueIdx = 1;
            string namePrefix = "<Template";
            string pt = HostRtbViewModel.CopyItem.ItemData.ToPlainText();
            while (pt.ToLower().Contains(namePrefix.ToLower() + uniqueIdx) ||
                   HostRtbViewModel
                    .TemplateHyperlinkCollectionViewModel
                    .Templates.Where(x => x.TemplateName == namePrefix + uniqueIdx + ">")
                    .ToList().Count > 0) {
                uniqueIdx++;
            }
            return namePrefix + uniqueIdx + ">";
        }

        #endregion

        #region Commands
        public ICommand ClearAllTemplatesCommand {
            get {
                return new RelayCommand(
                    () => {
                        foreach(var thlvm in Templates) {
                            thlvm.ClearTemplateCommand.Execute(null);
                        }
                    },
                    () => {
                        return Templates.Any(x=>x.HasText);
                    });
            }
        }

        public ICommand SelectNextTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        if(!SelectedTemplate.HasText) {
                            SelectedTemplate.SetTemplateText(" ");
                        }
                        int nextIdx = Templates.IndexOf(SelectedTemplate) + 1;
                        if (nextIdx >= Templates.Count) {
                            nextIdx = 0;
                        }
                        Templates[nextIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return HostRtbViewModel != null && 
                               HostClipTileViewModel.IsPastingTemplate &&
                               Templates.Count > 1;
                    });
            }
        }

        public ICommand SelectPreviousTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        if (!SelectedTemplate.HasText) {
                            SelectedTemplate.SetTemplateText(" ");
                        }
                        int prevIdx = Templates.IndexOf(SelectedTemplate) - 1;
                        if (prevIdx < Templates.Count) {
                            prevIdx = Templates.Count - 1;
                        }
                        Templates[prevIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return HostRtbViewModel != null && 
                               HostClipTileViewModel.IsPastingTemplate &&
                               Templates.Count > 1;
                    });
            }
        }

        public ICommand PasteTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        string rtf = HostRtbViewModel.CopyItem.ItemData;
                        foreach (var thlvm in Templates) {
                            rtf = rtf.Replace(thlvm.TemplateName, thlvm.TemplateText);
                        }
                        HostRtbViewModel.TemplateRichText = rtf;
                    },
                    () => {
                        return IsAllTemplatesFilled;
                    });
            }
        }
        #endregion
    }
}
