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
using MonkeyPaste.Plugin;
using System.Windows;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpTemplateCollectionViewModel : 
        MpSelectorViewModelBase<MpContentItemViewModel,MpTemplateViewModel>,
        MpIMenuItemViewModel {
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

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                var mivm = new MpMenuItemViewModel();
                mivm.SubItems = new List<MpMenuItemViewModel>();
                foreach(var tvm in Items) {
                    var tvm_mi = new MpMenuItemViewModel() {
                        Header = tvm.TemplateName,
                        IconHexStr = tvm.TemplateHexColor,
                        Command = CreateTemplateViewModelCommand,
                        CommandParameter = tvm
                    };
                    mivm.SubItems.Add(tvm_mi);
                }
                var ntvm_mi = new MpMenuItemViewModel() {
                    Header = "Add New",
                    IconResourceKey = Application.Current.Resources["AddIcon"] as string,
                    Command = CreateTemplateViewModelCommand,
                    CommandParameter = null
                };
                mivm.SubItems.Add(ntvm_mi);
                return mivm;
            }
        }
        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates => Items.Count > 1;

        public string PasteButtonText => IsAllTemplatesFilled ? "PASTE" : "CONTINUE";

        //public int SelectedTemplateIdx {
        //    get {
        //        return Items.IndexOf(Items.Where(x => x.IsSelected).FirstOrDefault());
        //    }
        //    set {
        //        if(value != SelectedTemplateIdx) {
        //            if(value >= 0 && value < Items.Count) {
        //                ClearSelection();
        //                Items[value].IsSelected = true;
        //            }

        //        }
        //    }
        //}
        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);
        public bool IsAllTemplatesFilled => Items.All(x => x.HasText);

        public bool IsAnyTemplateHasText => Items.Any(x => x.HasText);
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

        public bool IsAnyEditingTemplate => Items.Any(x => x.IsEditingTemplate);

        #endregion

        #endregion

        #region Constructors
        public MpTemplateCollectionViewModel() : base(null) { }

        public MpTemplateCollectionViewModel(MpContentItemViewModel rtbvm) : base(rtbvm) {
            HostClipTileViewModel.PropertyChanged += HostClipTileViewModel_PropertyChanged;
            //templates are added in the CreateHyperlinks rtb extension
        }

        #endregion

        #region Public Methods

        //public async Task InitializeAsync(int ciid) {
        //    IsBusy = true;

        //    var citl = await MpDataModelProvider.GetTextTemplatesAsync(ciid);
        //    foreach(var cit in citl) {
        //        var citvm = await CreateTemplateViewModel(cit);
        //        base.Items.Add(citvm);
        //    }
        //    base.OnPropertyChanged(nameof(MpSelectorViewModelBase<MpContentItemViewModel, MpTemplateViewModel>.Items));

        //    IsBusy = false;
        //}


        public async Task<MpTemplateViewModel> CreateTemplateViewModel(MpTextTemplate cit) {
            MpTemplateViewModel tvm = new MpTemplateViewModel(this);
            await tvm.InitializeAsync(cit);
            return tvm;
        }

        #endregion

        #region Private Methods

        private void HostClipTileViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HostClipTileViewModel.IsAnyPastingTemplate):
                    //ResetAll();
                    break;
            }
        }

        public void ClearAllEditing() {
            foreach(var thlvm in Items) {
                thlvm.IsEditingTemplate = false;
            }
        }

        public void ClearSelection() {
            foreach(var thlvm in Items) {
                thlvm.IsSelected = false;
            }
        }

        public void ResetAll() {
            foreach (var thlvm in Items) {
                thlvm.Reset();
            }
            ClearSelection();
        }


        private void Ntvm_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var thlvm = sender as MpTemplateViewModel;
            switch(e.PropertyName) {
                case nameof(thlvm.IsSelected):
                    if(thlvm.IsSelected) {
                        Items.Where(x => x != thlvm).ForEach(x => x.IsSelected = false);
                    }
                    OnPropertyChanged(nameof(SelectedItem));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.IsDetailGridVisibile));
                    break;
                case nameof(thlvm.IsEditingTemplate):
                    if(thlvm.IsEditingTemplate) {
                        foreach(var vm in Items) {
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
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.IsDetailGridVisibile));
                    break;
            }
            UpdateCommandsCanExecute();
        }

        public async Task<bool> RemoveItem(MpTextTemplate cit, bool removeAll) {
            MpConsole.WriteLine("Removing template: " + cit.TemplateName);
            //returns true if this was the last instance of the template
            var thlvmToRemove = Items.Where(x => x.TextTemplateId == cit.Id).FirstOrDefault();
            if(thlvmToRemove != null) {
                if(removeAll || thlvmToRemove.InstanceCount == 1) {
                    await thlvmToRemove.TextTemplate.DeleteFromDatabaseAsync();
                    Items.Remove(thlvmToRemove);
                } else {
                    thlvmToRemove.InstanceCount--;
                }
                return thlvmToRemove.InstanceCount == 0;
            }
            return false;
        }

        public string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string uniqueName = $"Template #";
            string testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
            string pt = Parent.CopyItem.ItemData.ToPlainText().ToLower();
            while (pt.Contains(testName) || Items.Any(x => x.TemplateName.ToLower() == testName)) {
                uniqueIdx++;
                testName = string.Format(
                                        @"{0}{1}",
                                        uniqueName.ToLower(),
                                        uniqueIdx);
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
            foreach (var thlvm in Items) {
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
                if(ci.Id == Parent.CopyItemId && Items != null) {
                    foreach(var cit in Items) {
                        await MpDb.DeleteItemAsync<MpTextTemplate>(cit.TextTemplate);
                    }
                }
            } //else if(e is MpTextTemplate cit && Items.Any(x=>x.TextTemplateId == cit.Id)) {
                // NOTE template model is deleted in LoadTemplates
            //    var toRemove_tvm = Items.FirstOrDefault(x => x.TextTemplateId == cit.Id);
            //    Items.Remove(toRemove_tvm);
            //    OnPropertyChanged(nameof(Items));
            //}
        }
        #endregion

        #region Private Methods

        #region Selection Changed Handlers

        private void Ntvm_OnTemplateSelected(object sender, EventArgs e) {
            var sthlvm = sender as MpTemplateViewModel;
            foreach (var thlvm in Items) {
                if (thlvm != sthlvm) {
                    thlvm.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(SelectedItem));
            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.IsDetailGridVisibile));
        }

        #endregion

        #region Db Event Handlers

        #endregion



        #endregion

        #region Commands
        public ICommand CreateTemplateViewModelCommand => new RelayCommand<object>(
            async (templateVmArg) => {
                string templateGuid = string.Empty;
                if(templateVmArg == null) {
                    string templateName = Parent.Parent.SelectedPlainText;
                    if(string.IsNullOrWhiteSpace(templateName)) {
                        templateName = GetUniqueTemplateName();
                    }
                    var cit = await MpTextTemplate.Create(
                        copyItemId: Parent.CopyItemId,
                        templateName: templateName);
                    templateGuid = cit.Guid;
                } else if (templateVmArg is MpTemplateViewModel tvm) {
                    templateGuid = tvm.TextTemplateGuid;
                } else {
                    return;
                }

                Parent.Parent.SelectedPlainText = "{t{"+templateGuid+"}t}";

                var ctvl = Application.Current.MainWindow.GetVisualDescendents<MpContentView>();
                if(ctvl == null) {
                    Debugger.Break();
                }
                var ctv = ctvl.FirstOrDefault(x => x.DataContext == Parent.Parent);
                if(ctv == null) {
                    Debugger.Break();
                }

                await MpMergedDocumentRtfExtension.LoadTemplates(ctv.Rtb);

                if(templateVmArg == null) {
                    //for new templates go into edit mode by default
                    var ntvm = Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid);
                    if(ntvm == null) {
                        Debugger.Break();
                    }
                    ntvm.EditTemplateCommand.Execute(null);
                }

            });

        public ICommand ClearAllTemplatesCommand => new RelayCommand(
            () => {
                foreach (var thlvm in Items) {
                    thlvm.ClearTemplateCommand.Execute(null);
                }
            },
            () => {
                return Items.Any(x=>x.HasText);
            });

        public ICommand SelectNextTemplateCommand => new RelayCommand(
            () => {
                //if (!SelectedTemplate.HasText) {
                //    SelectedTemplate.MatchData = " ";
                //}
                int nextIdx = Items.IndexOf(SelectedItem) + 1;
                if (nextIdx >= Items.Count) {
                    nextIdx = 0;
                }
                Items[nextIdx].IsSelected = true;
            });

        public ICommand SelectPreviousTemplateCommand => new RelayCommand(
            () => {
                //if (!SelectedTemplate.HasText) {
                //    SelectedTemplate.MatchData = " ";
                //}
                int prevIdx = Items.IndexOf(SelectedItem) - 1;
                if (prevIdx < 0) {
                    prevIdx = Items.Count - 1;
                }
                Items[prevIdx].IsSelected = true;
            });

        public ICommand PasteTemplateCommand => new RelayCommand(
            () => {
                string rtf = Parent.CopyItem.ItemData;
                MpConsole.WriteLine("Unmodified item rtf: ");
                MpConsole.WriteLine(rtf);
                foreach (var thlvm in Items) {
                    rtf = rtf.Replace(thlvm.TextTemplate.EncodedTemplate, thlvm.TemplateText);
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
            Items.ForEach(x => sb.AppendLine(x.ToString()));
            return sb.ToString();
        }
        #endregion
    }
}
