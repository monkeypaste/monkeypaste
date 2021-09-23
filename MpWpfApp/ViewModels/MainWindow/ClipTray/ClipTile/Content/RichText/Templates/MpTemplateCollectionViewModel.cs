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
    public class MpTemplateCollectionViewModel : MpViewModelBase<MpContentItemViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.Parent as MpClipTileViewModel;
            }
        }

        private ObservableCollection<MpTemplateViewModel> _templates = new ObservableCollection<MpTemplateViewModel>();
        public ObservableCollection<MpTemplateViewModel> Templates {
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

        public MpTemplateViewModel SelectedTemplate {
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
        public MpTemplateCollectionViewModel() : base(null) { }

        public MpTemplateCollectionViewModel(MpContentItemViewModel rtbvm) : base(rtbvm) {           
            Templates.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(Templates));
            };


            HostClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(HostClipTileViewModel.IsAnyPastingTemplate):
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

        public MpTemplateViewModel AddItem(MpCopyItemTemplate ncit) {
            MpTemplateViewModel ntvm = null;

            //check if template exists (it should)
            var dupCheck = Templates.Where(x => x.TemplateName == ncit.TemplateName).FirstOrDefault();
            if (dupCheck == null) {
                //not sure how this could happen but it may dunno
                ntvm = new MpTemplateViewModel(this, ncit);
            } else {
                //set existing thvm for return
                ntvm = dupCheck;
            }
            if (ntvm.InstanceCount == 0) {
                //only add one selection handler
                ntvm.OnTemplateSelected += Ntvm_OnTemplateSelected;
                Templates.Add(ntvm);
            }
            ntvm.InstanceCount++;

            return ntvm;
        }

        public string GetFormattedTemplateName(string text) {
            if (text == null) {
                text = string.Empty;
            }
            if (!text.StartsWith("<")) {
                text = "<" + text;
            }
            if (!text.EndsWith(">")) {
                text = text + ">";
            }
            return text;
        }

        public void RemoveItem(MpCopyItemTemplate cit, bool removeAll) {
            var thlvmToRemove = Templates.Where(x => x.CopyItemTemplateId == cit.Id).FirstOrDefault();
            if(thlvmToRemove != null) {
                if(removeAll || thlvmToRemove.InstanceCount == 1) {
                    thlvmToRemove.CopyItemTemplate.DeleteFromDatabase();
                    Templates.Remove(thlvmToRemove);
                } else {
                    thlvmToRemove.InstanceCount--;
                }
            }
        }

        public string GetUniqueTemplateName() {
            Parent.SaveToDatabase();
            int uniqueIdx = 1;
            string namePrefix = "<Template";
            string pt = Parent.CopyItem.ItemData.ToPlainText();
            while (pt.ToLower().Contains(namePrefix.ToLower() + uniqueIdx) ||
                   Parent
                    .TemplateCollection
                    .Templates.Where(x => x.TemplateName == namePrefix + uniqueIdx + ">")
                    .ToList().Count > 0) {
                uniqueIdx++;
            }
            return namePrefix + uniqueIdx + ">";
        }
        #endregion

        #region Private Methods

        #region Selection Changed Handlers

        private void Ntvm_OnTemplateSelected(object sender, EventArgs e) {
            var sthlvm = sender as MpTemplateViewModel;
            foreach (var thlvm in Templates) {
                if (thlvm != sthlvm) {
                    thlvm.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(SelectedTemplate));
        }

        #endregion

        #region Db Event Handlers

        #endregion

        

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
                            SelectedTemplate.TemplateText = " ";
                        }
                        int nextIdx = Templates.IndexOf(SelectedTemplate) + 1;
                        if (nextIdx >= Templates.Count) {
                            nextIdx = 0;
                        }
                        Templates[nextIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return Parent != null && 
                               HostClipTileViewModel.IsAnyPastingTemplate &&
                               Templates.Count > 1;
                    });
            }
        }

        public ICommand SelectPreviousTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        if (!SelectedTemplate.HasText) {
                            SelectedTemplate.TemplateText = " ";
                        }
                        int prevIdx = Templates.IndexOf(SelectedTemplate) - 1;
                        if (prevIdx < Templates.Count) {
                            prevIdx = Templates.Count - 1;
                        }
                        Templates[prevIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return Parent != null && 
                               HostClipTileViewModel.IsAnyPastingTemplate &&
                               Templates.Count > 1;
                    });
            }
        }

        public ICommand PasteTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        string rtf = Parent.CopyItem.ItemData;
                        foreach (var thlvm in Templates) {
                            rtf = rtf.Replace(thlvm.TemplateName, thlvm.TemplateText);
                        }
                        Parent.TemplateRichText = rtf;
                    },
                    () => {
                        return IsAllTemplatesFilled;
                    });
            }
        }
        #endregion
    }
}
