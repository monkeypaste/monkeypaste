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

        [MpChildViewModel(typeof(MpTemplateViewModel),true)]
        public ObservableCollection<MpTemplateViewModel> Templates { get; set; } = new ObservableCollection<MpTemplateViewModel>();

        public MpTemplateViewModel SelectedTemplate => Templates.Where(x => x.IsSelected).FirstOrDefault();
        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates => Templates.Count > 1;

        public string PasteButtonText => IsAllTemplatesFilled ? "PASTE" : "CONTINUE";

        public int SelectedTemplateIdx {
            get {
                return Templates.IndexOf(Templates.Where(x => x.IsSelected).FirstOrDefault());
            }
            set {
                if(value != SelectedTemplateIdx) {
                    if(value >= 0 && value < Templates.Count) {
                        ClearSelection();
                        Templates[value].IsSelected = true;
                    }

                }
            }
        }
        #endregion

        #region State
        public bool IsAllTemplatesFilled => Templates.All(x => x.HasText);

        public bool IsAnyTemplateHasText => Templates.Any(x => x.HasText);
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
                    //ResetAll();
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
                                //vm.OnPropertyChanged(nameof(vm.IsEditingTemplate));
                            }
                        }
                    } else {
                        thlvm.IsSelected = false;
                    }
                    OnPropertyChanged(nameof(IsAnyEditingTemplate));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.DetailGridVisibility));
                    break;
            }
            UpdateCommandsCanExecute();
        }

        public bool RemoveItem(MpCopyItemTemplate cit, bool removeAll) {
            MpConsole.WriteLine("Removing template: " + cit.TemplateName);
            //returns true if this was the last instance of the template
            var thlvmToRemove = Templates.Where(x => x.CopyItemTemplateId == cit.Id).FirstOrDefault();
            if(thlvmToRemove != null) {
                if(removeAll || thlvmToRemove.InstanceCount == 1) {
                    thlvmToRemove.CopyItemTemplate.DeleteFromDatabase();
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

        public void UpdateCommandsCanExecute() {
            //(ClearAllTemplatesCommand as RelayCommand).NotifyCanExecuteChanged();
            //(SelectNextTemplateCommand as RelayCommand).NotifyCanExecuteChanged();
            //(SelectPreviousTemplateCommand as RelayCommand).NotifyCanExecuteChanged();
            //(PasteTemplateCommand as RelayCommand).NotifyCanExecuteChanged();
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

        #region Protected Methods
        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if(ci.Id == Parent.CopyItemId && Templates != null) {
                    foreach(var cit in Templates) {
                        await MpDb.Instance.DeleteItemAsync<MpCopyItemTemplate>(cit.CopyItemTemplate);
                    }
                }
            }
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
            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.DetailGridVisibility));
        }

        #endregion

        #region Db Event Handlers

        #endregion

        

        #endregion

        #region Commands
        public ICommand ClearAllTemplatesCommand => new RelayCommand(
            () => {
                foreach (var thlvm in Templates) {
                    thlvm.ClearTemplateCommand.Execute(null);
                }
            },
            () => {
                return Templates.Any(x=>x.HasText);
            });

        public ICommand SelectNextTemplateCommand => new RelayCommand(
            () => {
                //if (!SelectedTemplate.HasText) {
                //    SelectedTemplate.TemplateText = " ";
                //}
                int nextIdx = Templates.IndexOf(SelectedTemplate) + 1;
                if (nextIdx >= Templates.Count) {
                    nextIdx = 0;
                }
                Templates[nextIdx].IsSelected = true;
            });

        public ICommand SelectPreviousTemplateCommand => new RelayCommand(
            () => {
                //if (!SelectedTemplate.HasText) {
                //    SelectedTemplate.TemplateText = " ";
                //}
                int prevIdx = Templates.IndexOf(SelectedTemplate) - 1;
                if (prevIdx < 0) {
                    prevIdx = Templates.Count - 1;
                }
                Templates[prevIdx].IsSelected = true;
            });

        public ICommand PasteTemplateCommand => new RelayCommand(
            () => {
                string rtf = Parent.CopyItem.ItemData;
                MpConsole.WriteLine("Unmodified item rtf: ");
                MpConsole.WriteLine(rtf);
                foreach (var thlvm in Templates) {
                    rtf = rtf.Replace(thlvm.CopyItemTemplate.TemplateToken, thlvm.TemplateText);
                }
                Parent.TemplateRichText = rtf;
                MpConsole.WriteLine("Pastable rtf: ");
                MpConsole.WriteLine(rtf);
            },
            () => {
                return IsAllTemplatesFilled;
            });

        #endregion

        #region Overrides

        public override string ToString() {
            var sb = new StringBuilder();
            Templates.ForEach(x => sb.AppendLine(x.ToString()));
            return sb.ToString();
        }
        #endregion
    }
}
