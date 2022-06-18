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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Windows;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpTemplateCollectionViewModel : 
        MpSelectorViewModelBase<MpClipTileViewModel,MpTextTemplateViewModelBase>,
        MpIMenuItemViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

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
                    Command = CreateTemplateViewModelCommand
                };
                mivm.SubItems.Add(ntvm_mi);
                return mivm;
            }
        }

        public ObservableCollection<MpTextTemplateViewModelBase> PastableItems { get; set; } = new ObservableCollection<MpTextTemplateViewModelBase>();

        #endregion

        #region Business Logic Properties
        public bool HasMultipleTemplates => Items.Where(x=>x.IsEnabled).Count() > 1;

        public string PasteButtonText => IsAllTemplatesFilled ? "PASTE" : "CONTINUE";

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);
        public bool IsAllTemplatesFilled => Items.Where(x => x.IsEnabled).All(x => x.HasText);

        public bool IsAnyTemplateHasText => Items.Where(x => x.IsEnabled).Any(x => x.HasText);

        public bool IsAnyEditingTemplate => Items.Any(x => x.IsEditingTemplate);

        #endregion

        #endregion

        #region Constructors
        public MpTemplateCollectionViewModel() : base(null) { }

        public MpTemplateCollectionViewModel(MpClipTileViewModel ctvm) : base(ctvm) { }

        #endregion

        #region Public Methods

        public async Task<MpTextTemplateViewModelBase> CreateTemplateViewModel(MpTextTemplate cit) {
            MpTextTemplateViewModelBase tvm = new MpTextTemplateViewModelBase(this);
            await tvm.InitializeAsync(cit);
            return tvm;
        }

        #endregion

        #region Private Methods


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

        public void Reset() {
            PastableItems.Clear();
            foreach (var thlvm in Items) {
                thlvm.Reset();
            }
            ClearSelection();
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


        #endregion

        #region Protected Methods
        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if(ci.Id == Parent.CopyItemId && base.Items != null) {
                    foreach(var cit in base.Items) {
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
            var sthlvm = sender as MpTextTemplateViewModelBase;
            foreach (var thlvm in Items) {
                if (thlvm != sthlvm) {
                    thlvm.IsSelected = false;
                }
            }
            OnPropertyChanged(nameof(SelectedItem));
            Parent.OnPropertyChanged(nameof(Parent.IsDetailGridVisibile));
        }

        #endregion

        #region Db Event Handlers

        #endregion



        #endregion

        #region Commands
        public ICommand CreateTemplateViewModelCommand => new RelayCommand<object>(
            async (templateVmOrSelectedTextArg) => {
                string templateGuid = string.Empty;
                if(templateVmOrSelectedTextArg == null) {
                    string templateName = Parent.SelectedPlainText;
                    if(string.IsNullOrWhiteSpace(templateName)) {
                        templateName = GetUniqueTemplateName();
                    }
                    string initialFormat = string.Empty;
                    var selectionFormat = Parent.SelectedRichTextFormat;
                    if(selectionFormat != null && selectionFormat.inlineFormat != null) {
                        initialFormat = selectionFormat.Serialize();
                    }
                    var cit = await MpTextTemplate.Create(
                        templateName: templateName,
                        rtfFormatJson: initialFormat);

                    templateGuid = cit.Guid;
                } else if (templateVmOrSelectedTextArg is MpTextTemplateViewModelBase tvm) {
                    templateGuid = tvm.TextTemplateGuid;
                } else {
                    return;
                }

                MpTextSelectionRangeExtension.SetSelectionText(Parent, "{t{" + templateGuid + "}t}");

                var ctvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>();
                if(ctvl == null) {
                    Debugger.Break();
                }
                var ctv = ctvl.FirstOrDefault(x => x.DataContext == Parent);
                if(ctv == null) {
                    Debugger.Break();
                }

                await MpContentDocumentRtfExtension.LoadTemplates(ctv.Rtb);

                if(templateVmOrSelectedTextArg == null) {
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
                while(!Items[nextIdx].IsEnabled) {
                    nextIdx++;
                    if (nextIdx >= Items.Count) {
                        nextIdx = 0;
                    }
                }
                SelectedItem = Items[nextIdx];
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
                while (!Items[prevIdx].IsEnabled) {
                    prevIdx--;
                    if (prevIdx < 0) {
                        prevIdx = Items.Count - 1;
                    }
                }
                SelectedItem = Items[prevIdx];
            });

        public ICommand PasteTemplateCommand => new RelayCommand(
            () => {
                var cv = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>()
                            .FirstOrDefault(x => x.DataContext == Parent);
                if(cv == null) {
                    Debugger.Break();
                }

                Parent.IsBusy = true;
                EventHandler hideEvent = null;
                hideEvent = (s, e) => {
                    Parent.IsBusy = false;
                    MpMainWindowViewModel.Instance.OnMainWindowHidden -= hideEvent;
                };

                MpMainWindowViewModel.Instance.OnMainWindowHidden += hideEvent;

                var rtb = cv.Rtb;
                string rtf = MpContentDocumentRtfExtension.GetEncodedContent(rtb,false);

                MpConsole.WriteLine("Unmodified item rtf: ");
                MpConsole.WriteLine(rtf);
                //Debugger.Break();
                foreach (var thlvm in Items) {
                    rtf = rtf.Replace(thlvm.TextTemplate.EncodedTemplateRtf, thlvm.TemplateText);
                }
                Parent.TemplateRichText = rtf;
                MpConsole.WriteLine("Pastable rtf: ");
                MpConsole.WriteLine(rtf);
            },
            () => IsAllTemplatesFilled);

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
