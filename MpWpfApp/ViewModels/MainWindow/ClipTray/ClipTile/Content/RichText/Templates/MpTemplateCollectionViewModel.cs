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
                return Parent.Parent;
            }
        }

        public ObservableCollection<MpTemplateViewModel> Templates { get; set; } = new ObservableCollection<MpTemplateViewModel>();

        public MpTemplateViewModel SelectedTemplate => Templates.Where(x => x.IsSelected).FirstOrDefault();
        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates => Templates.Count > 1;

        public string PasteButtonText => IsAllTemplatesFilled ? "PASTE" : "CONTINUE";

        #endregion

        #region State
        public bool IsAllTemplatesFilled => Templates.Any(x => x.HasText);

        //public int SelectedTemplateIdx {
        //    get {
        //        if(SelectedTemplate == null) {
        //            return 0;
        //        }
        //        return Templates.IndexOf(SelectedTemplate);
        //    }
        //    set {
        //        if(value >= 0 && value < Templates.Count) {
        //            ClearSelection();
        //            Templates[value].IsSelected = true;
        //        }
        //    }
        //}

        public bool IsAnyEditingTemplate => Templates.Any(x => x.IsEditingTemplate);
        #endregion

        #endregion

        #region Public Methods
        public MpTemplateCollectionViewModel() : base(null) { }

        public MpTemplateCollectionViewModel(MpContentItemViewModel rtbvm) : base(rtbvm) {
            HostClipTileViewModel.PropertyChanged += HostClipTileViewModel_PropertyChanged;
            //templates are added in the CreateHyperlinks rtb extension
        }

        private void HostClipTileViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HostClipTileViewModel.IsAnyPastingTemplate):
                    ResetAll();
                    break;
            }
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

        public MpTemplateViewModel CreateTemplateViewModel(MpCopyItemTemplate ncit) {
            MpTemplateViewModel ntvm = null;

            //check if template exists (it should)
            var dupCheck = Templates.Where(x => x.TemplateName == ncit.TemplateName).FirstOrDefault();
            if (dupCheck == null) {
                //not sure how this could happen but it may dunno
                ntvm = new MpTemplateViewModel(this, ncit);
                ntvm.PropertyChanged += Ntvm_PropertyChanged;
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

        private void Ntvm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var thlvm = sender as MpTemplateViewModel;
            switch(e.PropertyName) {
                case nameof(thlvm.IsSelected):
                    if(thlvm.IsSelected) {
                        Templates.Where(x => x != thlvm).ForEach(x => x.IsSelected = false);
                    }
                    OnPropertyChanged(nameof(SelectedTemplate));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.DetailGridVisibility));
                    break;
                case nameof(thlvm.IsEditingTemplate):
                    if(thlvm.IsEditingTemplate) {
                        foreach(var vm in Templates) {
                            if (vm == thlvm) {
                                vm.IsSelected = true;
                            } else {
                                vm.IsSelected = false;
                                vm.IsEditingTemplate = false;
                                vm.OnPropertyChanged(nameof(vm.IsEditingTemplate));
                            }
                        }
                    } else {
                        thlvm.IsSelected = false;
                    }
                    OnPropertyChanged(nameof(IsAnyEditingTemplate));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.DetailGridVisibility));
                    break;
            }
        }

        public string GetFormattedTemplateName(string text) {
            if (text == null) {
                text = string.Empty;
            }
            
            //if (!text.StartsWith(MpCopyItemTemplate.TEMPLATE_PREFIX)) {
            //    text = MpCopyItemTemplate.TEMPLATE_PREFIX + text;
            //}
            //if (!text.EndsWith(MpCopyItemTemplate.TEMPLATE_SUFFIX)) {
            //    text = text + MpCopyItemTemplate.TEMPLATE_SUFFIX;
            //}
            return text;
        }

        public bool RemoveItem(MpCopyItemTemplate cit, bool removeAll) {
            //returns true if this was the last instance of the template
            var thlvmToRemove = Templates.Where(x => x.CopyItemTemplateId == cit.Id).FirstOrDefault();
            if(thlvmToRemove != null) {
                if(removeAll || thlvmToRemove.InstanceCount == 1) {
                    thlvmToRemove.CopyItemTemplate.DeleteFromDatabase();
                    thlvmToRemove.InstanceCount = 0;
                    Templates.Remove(thlvmToRemove);
                } else {
                    thlvmToRemove.InstanceCount--;
                }
                return thlvmToRemove.InstanceCount == 0;
            }
            return false;
        }

        public string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string uniqueName = $"Template";
            string testName = string.Format(
                                        @"{0}{1}{2}{3}",
                                        MpCopyItemTemplate.TEMPLATE_PREFIX,
                                        uniqueName.ToLower(),
                                        uniqueIdx,
                                        MpCopyItemTemplate.TEMPLATE_SUFFIX);
            string pt = Parent.CopyItem.ItemData.ToPlainText().ToLower();
            while (pt.Contains(testName) || Templates.Any(x => x.TemplateDisplayValue.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}{2}{3}",
                                        MpCopyItemTemplate.TEMPLATE_PREFIX,
                                        uniqueName.ToLower(),
                                        uniqueIdx,
                                        MpCopyItemTemplate.TEMPLATE_SUFFIX);
            }
            return uniqueName + uniqueIdx;
        }

        #region IDisposable

        public override void Dispose() {
            base.Dispose();
            HostClipTileViewModel.PropertyChanged -= HostClipTileViewModel_PropertyChanged;
            foreach (var thlvm in Templates) {
                thlvm.Dispose();
                thlvm.PropertyChanged -= Ntvm_PropertyChanged;
                thlvm.OnTemplateSelected -= Ntvm_OnTemplateSelected;
            }
        }

        #endregion

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
            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.DetailGridVisibility));
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
