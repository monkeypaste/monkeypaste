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
    public class MpTokenCollectionViewModel : MpViewModelBase<MpContentItemViewModel> {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.Parent.Parent as MpClipTileViewModel;
            }
        }

        private ObservableCollection<MpTokenViewModel> _templates = new ObservableCollection<MpTokenViewModel>();
        public ObservableCollection<MpTokenViewModel> Tokens {
            get {
                return _templates;
            }
            private set {
                if(_templates != value) {
                    _templates = value;
                    OnPropertyChanged(nameof(Tokens));
                }
            }
        }

        public MpTokenViewModel SelectedTemplate {
            get {
                return Tokens.Where(x => x.IsSelected).FirstOrDefault();
            }
        }
        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates {
            get {
                return Tokens.Count > 1;
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
                return Tokens.Any(x => x.HasText);
            }
        }
        public int SelectedTemplateIdx {
            get {
                if(SelectedTemplate == null) {
                    return 0;
                }
                return Tokens.IndexOf(SelectedTemplate);
            }
            set {
                if(value >= 0 && value < Tokens.Count) {
                    ClearSelection();
                    Tokens[value].IsSelected = true;
                }
            }
        }

        public bool IsEditingTemplate {
            get {
                return Tokens.Any(x => x.IsEditingTemplate);
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTokenCollectionViewModel() : base(null) { }

        public MpTokenCollectionViewModel(MpContentItemViewModel rtbvm) : base(rtbvm) {           
            Tokens.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(Tokens));
            };


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
            foreach(var thlvm in Tokens) {
                thlvm.IsEditingTemplate = false;
            }
        }

        public void ClearSelection() {
            foreach(var thlvm in Tokens) {
                thlvm.IsSelected = false;
            }
        }

        public void ResetAll() {
            foreach (var thlvm in Tokens) {
                thlvm.Reset();
            }
            ClearSelection();
        }

        public MpTokenViewModel AddItem(MpCopyItemTemplate ncit) {
            MpTokenViewModel ntvm = null;

            if (ncit == null) {
                //for new templates create a default name
                ncit = MpCopyItemTemplate.Create(
                            Parent.CopyItem.Id,
                            GetUniqueTemplateName());
                ncit.WriteToDatabase();
                ntvm = new MpTokenViewModel(this, ncit);
            } else {
                //check if template exists (it should)
                var dupCheck = Tokens.Where(x => x.TemplateName == ncit.TemplateName).FirstOrDefault();
                if (dupCheck == null) {
                    //not sure how this could happen but it may dunno
                    ntvm = new MpTokenViewModel(this, ncit);
                } else {
                    //set existing thvm for return
                    ntvm = dupCheck;
                }
            }
            if (ntvm.InstanceCount == 0) {
                //only add one selection handler
                ntvm.OnTemplateSelected += Ntvm_OnTemplateSelected;
                Tokens.Add(ntvm);
            }
            ntvm.InstanceCount++;

            return ntvm;
        }

        public void RemoveItem(MpCopyItemTemplate cit, bool removeAll) {
            var thlvmToRemove = Tokens.Where(x => x.CopyItemTemplateId == cit.Id).FirstOrDefault();
            if(thlvmToRemove != null) {
                if(removeAll || thlvmToRemove.InstanceCount == 1) {
                    thlvmToRemove.CopyItemTemplate.DeleteFromDatabase();
                    Tokens.Remove(thlvmToRemove);
                } else {
                    thlvmToRemove.InstanceCount--;
                }
            }
        }
        #endregion

        #region Private Methods

        #region Selection Changed Handlers

        private void Ntvm_OnTemplateSelected(object sender, EventArgs e) {
            var sthlvm = sender as MpTokenViewModel;
            foreach (var thlvm in Tokens) {
                if (thlvm != sthlvm) {
                    thlvm.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(SelectedTemplate));
        }

        #endregion

        #region Db Event Handlers

        #endregion

        private string GetUniqueTemplateName() {
            Parent.SaveToDatabase();
            int uniqueIdx = 1;
            string namePrefix = "<Template";
            string pt = Parent.CopyItem.ItemData.ToPlainText();
            while (pt.ToLower().Contains(namePrefix.ToLower() + uniqueIdx) ||
                   Parent
                    .TokenCollection
                    .Tokens.Where(x => x.TemplateName == namePrefix + uniqueIdx + ">")
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
                        foreach(var thlvm in Tokens) {
                            thlvm.ClearTemplateCommand.Execute(null);
                        }
                    },
                    () => {
                        return Tokens.Any(x=>x.HasText);
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
                        int nextIdx = Tokens.IndexOf(SelectedTemplate) + 1;
                        if (nextIdx >= Tokens.Count) {
                            nextIdx = 0;
                        }
                        Tokens[nextIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return Parent != null && 
                               HostClipTileViewModel.IsPastingTemplate &&
                               Tokens.Count > 1;
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
                        int prevIdx = Tokens.IndexOf(SelectedTemplate) - 1;
                        if (prevIdx < Tokens.Count) {
                            prevIdx = Tokens.Count - 1;
                        }
                        Tokens[prevIdx].IsSelected = true;
                        OnPropertyChanged(nameof(SelectedTemplateIdx));
                    },
                    () => {
                        return Parent != null && 
                               HostClipTileViewModel.IsPastingTemplate &&
                               Tokens.Count > 1;
                    });
            }
        }

        public ICommand PasteTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        string rtf = Parent.CopyItem.ItemData;
                        foreach (var thlvm in Tokens) {
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
