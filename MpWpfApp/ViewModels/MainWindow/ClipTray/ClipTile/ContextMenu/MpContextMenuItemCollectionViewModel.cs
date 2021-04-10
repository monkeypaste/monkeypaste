using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpContextMenuItemCollectionViewModel : MpUndoableObservableCollectionViewModel<MpContextMenuItemCollectionViewModel,MpContextMenuItemViewModel> {
        #region Private Variables

        #endregion

        #region Properties
        private MpRtbListBoxItemRichTextBoxViewModel _rtbvm = null;
        public MpRtbListBoxItemRichTextBoxViewModel Rtbvm {
            get {
                return _rtbvm;
            }
            set {
                if(_rtbvm != value) {
                    _rtbvm = value;
                    OnPropertyChanged(nameof(Rtbvm));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpContextMenuItemCollectionViewModel() : base() { }

        public MpContextMenuItemCollectionViewModel(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(Rtbvm):
                        Init();
                        break;
                }
            };
            Rtbvm = rtbvm;

            Rtbvm.ViewModelLoaded += (s, e) => {
                

                Rtbvm.Rtbc.ContextMenuOpening += (s1, e1) => {
                    var cm = Rtbvm.Rtbc.ContextMenu;
                    var cmi = new MenuItem();
                    cm.Items.Add(cmi);
                    MpHelpers.Instance.SetColorChooserMenuItem(
                        cm,
                        cmi,
                        (s2, e2) => {
                            MainWindowViewModel.ClipTrayViewModel.ChangeSelectedClipsColorCommand.Execute((Brush)((Border)s2).Tag);
                            foreach (var sctvm in MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles) {
                                sctvm.CopyItem.WriteToDatabase();
                            }
                        },
                        MpHelpers.Instance.GetColorColumn(Rtbvm.TitleBackgroundColor),
                        MpHelpers.Instance.GetColorRow(Rtbvm.TitleBackgroundColor)
                    );

                    cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);
                };
            };
        }


        public void Init() {
            this.Clear();
            if(Rtbvm == null) {
                return;
            }
            new MpContextMenuItemViewModel(
                "_Paste",
                Rtbvm.RichTextBoxViewModelCollection.PasteSubSelectedClipsCommand,
                null,
                false,
                 Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/paste.png");


            this.Add(new MpContextMenuItemViewModel(
               "Delete",
               Rtbvm.RichTextBoxViewModelCollection.DeleteSubSelectedClipsCommand,
               null,
               false,
               Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/Delete.png",
               null,
               Rtbvm.ShortcutKeyString));

            this.Add(null);

            this.Add(new MpContextMenuItemViewModel(
                "_Edit",
                null,
                null,
                false,
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/edit.png",
                new ObservableCollection<MpContextMenuItemViewModel> {
                    new MpContextMenuItemViewModel(
                        "_Edit Content",
                        Rtbvm.RichTextBoxViewModelCollection.ToggleEditSubSelectedItemCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/caret.png"),
                    new MpContextMenuItemViewModel(
                        "_Rename",
                        Rtbvm.ToggleEditSubTitleCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/rename.png"),
                    null,
                    new MpContextMenuItemViewModel(
                        "_Translate",
                        null,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/translate.png",
                        Rtbvm.HostClipTileViewModel.TranslateLanguageMenuItems)
                }));

            this.Add(new MpContextMenuItemViewModel(
                "_Tools", 
                null,
                null, 
                false, 
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/tools.png",
                new ObservableCollection<MpContextMenuItemViewModel> {
                    null,
                    new MpContextMenuItemViewModel(
                        "_Duplicate",
                        Rtbvm.RichTextBoxViewModelCollection.DuplicateSubSelectedClipsCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/duplicate.png"),
                    new MpContextMenuItemViewModel(
                        "_Merge",
                        Rtbvm.RichTextBoxViewModelCollection.MergeSubSelectedClipsCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/merge.png"), 
                    null,
                    new MpContextMenuItemViewModel(
                        "Send As Email",
                        Rtbvm.RichTextBoxViewModelCollection.DuplicateSubSelectedClipsCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/email.png"),
                    new MpContextMenuItemViewModel(
                        "Duplicate As _Qr Code",
                        Rtbvm.CreateQrCodeFromSubSelectedItemCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/qr.png"),
                    null,
                    new MpContextMenuItemViewModel(
                        "_Speak",
                        Rtbvm.RichTextBoxViewModelCollection.SpeakSubSelectedClipsAsyncCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/speak.png"),
                    null,
                    new MpContextMenuItemViewModel(
                        "_Search",
                        null,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/web.png",
                        new ObservableCollection<MpContextMenuItemViewModel>{ 
                            new MpContextMenuItemViewModel(
                                "_Google",
                                Rtbvm.RichTextBoxViewModelCollection.SearchWebCommand,
                                @"https://www.google.com/search?q=",
                                false,
                                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/googleIcon.png"),
                            new MpContextMenuItemViewModel(
                                "_Bing",
                                Rtbvm.RichTextBoxViewModelCollection.SearchWebCommand,
                                @"https://www.bing.com/search?q=",
                                false,
                                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/bingIcon.png"),
                            new MpContextMenuItemViewModel(
                                "_Duck Duck Go",
                                Rtbvm.RichTextBoxViewModelCollection.SearchWebCommand,
                                @"https://duckduckgo.com/?q=",
                                false,
                                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/duckduckgoIcon.png"),
                            new MpContextMenuItemViewModel(
                                "_Yandex",
                                Rtbvm.RichTextBoxViewModelCollection.SearchWebCommand,
                                @"ho",
                                false,
                                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/yandexIcon.png")
                        })
                }));            

            this.Add(new MpContextMenuItemViewModel(
                "_Selection",
                null,
                null,
                false,
                 Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/selection.png",
                 new ObservableCollection<MpContextMenuItemViewModel>{
                     new MpContextMenuItemViewModel(
                        "_Bring To Front",
                        Rtbvm.RichTextBoxViewModelCollection.BringSubSelectedClipTilesToFrontCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/bringToFront.png"),
                     new MpContextMenuItemViewModel(
                        "_Send To Back",
                        Rtbvm.RichTextBoxViewModelCollection.SendSubSelectedClipTilesToBackCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/sendToBack.png"),
                     null,
                     new MpContextMenuItemViewModel(
                        "Select _All",
                        Rtbvm.RichTextBoxViewModelCollection.BringSubSelectedClipTilesToFrontCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/selectall.png"),
                     new MpContextMenuItemViewModel(
                        "_Invert Selection",
                        Rtbvm.RichTextBoxViewModelCollection.InvertSubSelectionCommand,
                        null,
                        false,
                        Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/invertSelection.png")
                     }));

            this.Add(new MpContextMenuItemViewModel(
               "Hotkey",
               Rtbvm.RichTextBoxViewModelCollection.AssignHotkeyCommand,
               null,
               false,
               Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/hotkey.png",
               null,
               Rtbvm.ShortcutKeyString));

            this.Add(null);

            this.Add(new MpContextMenuItemViewModel(
               "Paste To Application",
               null,
               null,
               false,
               Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/bullseye.png",
               null,
               string.Empty));

            this.Add(null);

            this.Add(new MpContextMenuItemViewModel(
               "Pin To Collection",
               null,
               null,
               false,
               Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/tag.png",
               Rtbvm.TagMenuItems));



        }
        #endregion

        #region Commands

        #endregion
    }
}
