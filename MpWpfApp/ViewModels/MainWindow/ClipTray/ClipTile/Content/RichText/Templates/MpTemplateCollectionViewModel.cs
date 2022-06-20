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
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Wpf;
using System.Windows;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MpWpfApp {
    public class MpTemplateCollectionViewModel : 
        MpSelectorViewModelBase<MpClipTileViewModel,MpTextTemplateViewModelBase> {
        #region Private Variables

        #endregion

        #region Statics
        public static ObservableCollection<MpContact> Contacts { get; set; }
        #endregion

        #region Properties

        #region View Models

        //public MpMenuItemViewModel MenuItemViewModel {
        //    get {
        //        if(Parent == null) {
        //            return null;
        //        }
        //        var mivm = new MpMenuItemViewModel();
        //        mivm.SubItems = new List<MpMenuItemViewModel>();
        //        foreach(var tvm in Items) {
        //            var tvm_mi = new MpMenuItemViewModel() {
        //                Header = tvm.TemplateName,
        //                IconHexStr = tvm.TemplateHexColor,
        //                Command = CreateTemplateViewModelCommand,
        //                CommandParameter = tvm
        //            };
        //            mivm.SubItems.Add(tvm_mi);
        //        }
        //        var ntvm_mi = new MpMenuItemViewModel() {
        //            Header = "Add New",
        //            IconResourceKey = Application.Current.Resources["AddIcon"] as string,
        //            Command = CreateTemplateViewModelCommand
        //        };
        //        mivm.SubItems.Add(ntvm_mi);
        //        return mivm;
        //    }
        //}



        public ObservableCollection<MpTextTemplateViewModelBase> PastableItems { get; set; } = new ObservableCollection<MpTextTemplateViewModelBase>();

        public IEnumerable<MpTextTemplateViewModelBase> PastableItemsNeedingInput => PastableItems.Where(x => x.IsInputRequiredForPaste);
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
            MpTextTemplateViewModelBase tvm = null;
            switch(cit.TemplateType) {
                case MpTextTemplateType.Contact:
                    tvm = new MpContactTextTemplateViewModel(this);
                    break;
                case MpTextTemplateType.DateTime:
                    tvm = new MpDateTimeTextTemplateViewModel(this);
                    break;
                case MpTextTemplateType.Dynamic:
                    tvm = new MpDynamicTextTemplateViewModel(this);
                    break;
                case MpTextTemplateType.Static:
                    tvm = new MpStaticTextTemplateViewModel(this);
                    break;
            }

            await tvm.InitializeAsync(cit);
            return tvm;
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

        public void Reset() {
            PastableItems.Clear();
            foreach (var thlvm in Items) {
                thlvm.Reset();
            }
            ClearSelection();
        }


        public async Task<MpMenuItemViewModel> GetAddTemplateMenuItemViewModel() {
            var ttl = await MpDb.GetItemsAsync<MpTextTemplate>();
            var mivm = new MpMenuItemViewModel() {
                SubItems = new List<MpMenuItemViewModel>()
            };
            for (int i = 0; i < Enum.GetValues(typeof(MpTextTemplateType)).Length; i++) {
                if (i == 0) {
                    continue;
                }
                MpTextTemplateType templateType = (MpTextTemplateType)i;
                var templatesForType = ttl.Where(x => x.TemplateType == templateType);
                var subItem = new MpMenuItemViewModel() {
                    Header = templateType.ToString().ToLabel(),
                    IconResourceKey = GetTemplateTypeIconResourceStr(templateType),
                    SubItems = templatesForType
                                  .Select(x => new MpMenuItemViewModel() {
                                        Header = x.TemplateName,
                                        IconHexStr = x.HexColor,
                                        Command = CreateTemplateViewModelCommand,
                                        CommandParameter = x
                                    }).ToList()
                };
                if(templatesForType.Count() > 0) {
                    subItem.SubItems.Add(new MpMenuItemViewModel() { IsSeparator = true });
                }
                
                subItem.SubItems.Add(new MpMenuItemViewModel() {
                    Header = $"New {templateType.ToString().ToLabel()} Template...",
                    IconResourceKey = "PlusIcon",
                    Command = CreateTemplateViewModelCommand,
                    CommandParameter = templateType
                });
                mivm.SubItems.Add(subItem);
            }
            return mivm;
        }

        public string GetUniqueTemplateName(string prefix = "Template") {
            int uniqueIdx = 1;
            string uniqueName = $"{prefix} #";
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

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTextTemplate cit && Items.Any(x => x.TextTemplateId == cit.Id)) {
                //NOTE template model is deleted in LoadTemplates
                //var toRemove_tvm = Items.FirstOrDefault(x => x.TextTemplateId == cit.Id);
                //Items.Remove(toRemove_tvm);
                //OnPropertyChanged(nameof(Items));

                MpHelpers.RunOnMainThread(async () => {
                    var ctv = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>()
                                                            .FirstOrDefault(x => x.DataContext == Parent);
                    if (ctv == null) {
                        Debugger.Break();
                    }

                    await MpContentDocumentRtfExtension.LoadTemplates(ctv.Rtb);
                });
            }
        }

        #endregion

        #region Private Methods
        public string GetTemplateTypeIconResourceStr(MpTextTemplateType templateType) {
            switch (templateType) {
                case MpTextTemplateType.Contact:
                    return "ContactIcon";
                case MpTextTemplateType.DateTime:
                    return "AlarmClockIcon";
                case MpTextTemplateType.Dynamic:
                    return "YinYangIcon";
                case MpTextTemplateType.Static:
                    return "IceCubeIcon";
            }
            return string.Empty;
        }
        #region Selection Changed Handlers


        #endregion

        #endregion

        #region Commands
        public ICommand CreateTemplateViewModelCommand => new RelayCommand<object>(
            async (templateModelOrTemplateTypeArg) => {
                if(templateModelOrTemplateTypeArg == null) {
                    Debugger.Break();
                }

                string templateGuid = string.Empty;
                if(templateModelOrTemplateTypeArg is MpTextTemplateType templateType) {
                    string templateName = Parent.SelectedPlainText;

                    if (string.IsNullOrWhiteSpace(templateName)) {
                        templateName = GetUniqueTemplateName(templateType.ToString());
                    }

                    string initialFormat = string.Empty;
                    var selectionFormat = Parent.SelectedRichTextFormat;
                    if(selectionFormat != null && selectionFormat.inlineFormat != null) {
                        initialFormat = selectionFormat.Serialize();
                    }
                    var cit = await MpTextTemplate.Create(
                        templateName: templateName,
                        templateType: templateType,
                        rtfFormatJson: initialFormat);

                    templateGuid = cit.Guid;
                } else if (templateModelOrTemplateTypeArg is MpTextTemplate cit) {
                    templateGuid = cit.Guid;
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

                MpContentDocumentRtfExtension.LoadTemplates(ctv.Rtb).FireAndForgetSafeAsync(this);
                await Task.Delay(10);

                while(IsBusy) {
                    await Task.Delay(100);
                }

                var ntvm = Items.FirstOrDefault(x => x.TextTemplateGuid == templateGuid);
                if (ntvm == null) {
                    Debugger.Break();
                }
                ntvm.EditTemplateCommand.Execute(null);
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
                int nextIdx = Items.IndexOf(SelectedItem) + 1;
                if (nextIdx >= Items.Count) {
                    nextIdx = 0;
                }
                while(!Items[nextIdx].IsEnabled || !Items[nextIdx].IsInputRequiredForPaste) {
                    nextIdx++;
                    if (nextIdx >= Items.Count) {
                        nextIdx = 0;
                    }
                }
                SelectedItem = Items[nextIdx];
            });

        public ICommand SelectPreviousTemplateCommand => new RelayCommand(
            () => {
                int prevIdx = Items.IndexOf(SelectedItem) - 1;
                if (prevIdx < 0) {
                    prevIdx = Items.Count - 1;
                }
                while (!Items[prevIdx].IsEnabled || !Items[prevIdx].IsInputRequiredForPaste) {
                    prevIdx--;
                    if (prevIdx < 0) {
                        prevIdx = Items.Count - 1;
                    }
                }
                SelectedItem = Items[prevIdx];
            });

        public ICommand BeginPasteTemplateCommand => new RelayCommand<object>(
            (args) => {
                if(args is IEnumerable<MpTextTemplateViewModelBase> pastableItems) {
                    PastableItems = new ObservableCollection<MpTextTemplateViewModelBase>(pastableItems);
                    OnPropertyChanged(nameof(PastableItemsNeedingInput));
                    OnPropertyChanged(nameof(Items));
                    OnPropertyChanged(nameof(HasMultipleTemplates));

                    MpTextTemplateViewModelBase templateToSelect = null;
                    foreach(var tvm in PastableItems) {
                        if (tvm.IsInputRequiredForPaste) {
                            if (templateToSelect == null) {
                                templateToSelect = tvm;
                            }
                            continue;
                        }
                        tvm.FillAutoTemplate();

                    }
                    if(templateToSelect != null) {
                        SelectedItem = templateToSelect;
                    }
                }
                
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
                bool wasAllSelected = false;
                if(rtb.Selection.IsEmpty) {
                    rtb.SelectAll();
                    wasAllSelected = true;
                }
                string rtf = MpContentDocumentRtfExtension.GetEncodedContent(rtb,false);

                MpConsole.WriteLine("Unmodified item rtf: ");
                MpConsole.WriteLine(rtf);
                //Debugger.Break();
                foreach (var thlvm in Items) {
                    rtf = rtf.Replace(thlvm.TextTemplate.EncodedTemplateRtf, thlvm.TemplateText);
                }
                if(wasAllSelected) {
                    rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
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
