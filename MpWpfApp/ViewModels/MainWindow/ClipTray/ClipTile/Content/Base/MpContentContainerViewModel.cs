using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AsyncAwaitBestPractices.MVVM;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpContentContainerViewModel : MpUndoableViewModelBase<MpContentContainerViewModel> {
        #region Private Variables
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;
        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();
        #endregion 

        #region Properties

        #region Visibility
        public ScrollBarVisibility HorizontalScrollbarVisibility {
            get {
                if (HostClipTileViewModel == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if (TotalExpandedSize.Width > HostClipTileViewModel.TileContentWidth) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollbarVisibility {
            get {
                if (HostClipTileViewModel == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if (TotalExpandedSize.Height > ContainerSize.Height) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }
        #endregion

        #region Layout
        public Size ContainerSize {
            get {
                var cs = new Size(MpMeasurements.Instance.ClipTileScrollViewerWidth, 0);
                if (HostClipTileViewModel == null) {
                    return cs;
                }
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (HostClipTileViewModel.IsEditingTile) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (HostClipTileViewModel.IsPastingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (HostClipTileViewModel.IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                if (HostClipTileViewModel.DetailGridVisibility != Visibility.Visible) {
                    ch += HostClipTileViewModel.TileDetailHeight;
                }
                if (Count == 1) {
                    cs.Height = ch;
                } else {
                    double h = HostClipTileViewModel.IsExpanded ? TotalExpandedSize.Height : TotalUnexpandedSize.Height;
                    cs.Height = Math.Max(MpMeasurements.Instance.ClipTileScrollViewerWidth, Math.Max(ch, h));
                }

                return cs;
            }
        }

        public Size TotalExpandedSize {
            get {
                var ts = new Size(
                MpMeasurements.Instance.ClipTileEditModeMinWidth,
                0);
                foreach (var ivm in ItemViewModels) {
                    var ivs = ivm.GetExpandedSize();
                    ts.Width = Math.Max(ts.Width, ivs.Width);
                    ts.Height += ivs.Height;
                }
                return ts;
            }
        }


        public Size TotalUnexpandedSize {
            get {
                return new Size(
                MpMeasurements.Instance.ClipTileContentMinWidth,
                MpMeasurements.Instance.ClipTileContentHeight);
                //foreach (var ivm in ItemViewModels) {
                //    var ivs = ivm.GetExpandedSize();
                //    ts.Width = Math.Max(ts.Width, ivs.Width);
                //}
                //return ts;
            }
        }
        #endregion

        #region State
        public bool IsDynamicPaste {
            get {
                if (HeadItem == null ||
                HeadItem.CopyItem.ItemType != MpCopyItemType.RichText) {
                    return false;
                }
                foreach (var ivm in ItemViewModels) {
                    if (ivm is MpRtbItemViewModel && (ivm as MpRtbItemViewModel).HasTemplate) {
                        return true;
                    }
                }
                return false;
            }
        }

        public int Count {
            get {
                return ItemViewModels.Count;
            }
        }

        public bool IsAnyEditingContent {
            get {
                return ItemViewModels.Any(x => x.IsSubEditingContent);
            }
        }

        public bool IsAnyEditingTitle {
            get {
                return ItemViewModels.Any(x => x.IsSubEditingTitle);
            }
        }

        public bool IsAnyPastingTemplate {
            get {
                return ItemViewModels.Any(x => x.IsSubPastingTemplate);
            }
        }
        #endregion

        #endregion

        #region Events
        public event EventHandler OnUiUpdateRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        public event EventHandler<object> OnSubSelectionChanged;
        #endregion

        public async Task UserPreparingDynamicPaste() {
            await Task.Delay(1);
        }
        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            await Task.Delay(1);
            return "";
        }
        public void RequestScrollIntoView(object obj) {
            OnScrollIntoViewRequest?.Invoke(this, obj);
        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }
        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        public bool IsAnyContentDragging {
            get {
                return ItemViewModels.Any(x => x.IsSubDragging);
            }
        }

        public bool IsAnyContentDropping {
            get {
                return ItemViewModels.Any(x => x.IsSubDropping);
            }
        }

        public void ClearAllSubDragDropState() {
            foreach (var ivm in ItemViewModels) {
                ivm.ClearSubDragState();
            }
        }

        public bool IsAnySubContextMenuOpened {
            get {
                return ItemViewModels.Any(x => x.IsSubContextMenuOpen);
            }
        }

        public bool IsAnySubSelected {
            get {
                return ItemViewModels.Any(x => x.IsSubSelected);
            }
        }

        public void ResetSubSelection(List<MpContentItemViewModel> origSel = null) {
            ClearSubSelection();
            if (VisibleContentItems.Count > 0) {
                if(origSel == null) {
                    VisibleContentItems[0].IsSubSelected = true;
                } else {
                    foreach(var sivm in origSel) {
                        var ivm = ItemViewModels.Where(x => x.CopyItem.Id == sivm.CopyItem.Id).FirstOrDefault();
                        if(ivm == null) {
                            continue;
                        }
                        ivm.IsSubSelected = true;
                    }
                }
                
            }
        }

        public void ClearSubSelection() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSubSelected = false;
            }
        }

        public void ClearSubHovering() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSubHovering = false;
            }
        }

        public void SubSelectAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSubSelected = true;
            }
        }

        public MpContentItemViewModel GetContentItemByCopyItemId(int ciid) {
            return ItemViewModels.Where(x => x.CopyItem.Id == ciid).FirstOrDefault();
        }

        public MpContentItemViewModel HeadItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.OrderBy(x => x.CopyItem.CompositeSortOrderIdx).ToList()[0];
            }
        }
        public MpContentItemViewModel TailItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.OrderByDescending(x => x.CopyItem.CompositeSortOrderIdx).ToList()[0];
            }
        }

        public MpContentItemViewModel PrimarySubSelectedClipItem {
            get {
                if (SubSelectedContentItems == null || SubSelectedContentItems.Count < 1) {
                    return null;
                }
                return SubSelectedContentItems[0];
            }
        }

        public List<MpContentItemViewModel> SubSelectedContentItems {
            get {
                return ItemViewModels.Where(x => x.IsSubSelected == true).OrderBy(x => x.LastSubSelectedDateTime).ToList();
            }
        }

        public List<MpContentItemViewModel> VisibleContentItems {
            get {
                return ItemViewModels.Where(x => x.ItemVisibility == Visibility.Visible).ToList();
            }
        }

        public List<string> FileList {
            get {
                var fl = new List<string>();
                var ivml = SubSelectedContentItems.Count == 0 ? ItemViewModels.ToList() : SubSelectedContentItems;
                foreach (var ivm in ivml) {
                    fl.AddRange(ivm.GetFileList());
                }
                return fl;
            }
        }

        public string GetDetailText(MpCopyItemDetailType detailType) {
            return (HeadItem as MpRtbItemViewModel).GetDetail(detailType);
        }

        public ObservableCollection<MpContentItemViewModel> ItemViewModels { get; private set; } = new ObservableCollection<MpContentItemViewModel>();

        public MpClipTileViewModel HostClipTileViewModel { get; private set; }

        public void InsertRange(int idx, List<MpCopyItem> models) {
            idx = idx < 0 ? 0 : idx >= ItemViewModels.Count ? ItemViewModels.Count : idx;
            foreach(var ci in models) {
                switch(ci.ItemType) {
                    case MpCopyItemType.RichText:
                        ItemViewModels.Insert(idx, new MpRtbItemViewModel(this,ci));
                        break;
                }
            }
        }

        public void RemoveRange(List<MpCopyItem> models) {
            for (int i = 0; i < models.Count; i++) {
                var ivm = ItemViewModels.Where(x => x.CopyItem.Id == models[i].Id).FirstOrDefault();
                if(ivm != null) {
                    ItemViewModels.Remove(ivm);
                }                
            }
        }

        public void SaveAll() {
            foreach(var ivm in ItemViewModels) {
                ivm.SaveToDatabase();
            }            
        }

        public void DeleteAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.RemoveFromDatabase();
            }
        }

        public void RefreshSubItems() {
            if(HeadItem != null) {
                var ccil = MpCopyItem.GetCompositeChildren(HeadItem.CopyItem);
                foreach(var cci in ccil) {
                    var dupCheck = ItemViewModels.Where(x => x.CopyItem.Id == cci.Id).FirstOrDefault();
                    if(dupCheck == null) {
                        ItemViewModels.Add(MpContentItemViewModel.Create(this, cci));
                    } else {
                        ItemViewModels[ItemViewModels.IndexOf(dupCheck)].CopyItem = cci;
                    }                    
                }
                ItemViewModels = new ObservableCollection<MpContentItemViewModel>(ItemViewModels.OrderBy(x => x.CopyItem.CompositeSortOrderIdx).ToList());
            }
        }

        public virtual void Resize(double deltaTop, double deltaWidth, double deltaHeight) {
            RequestUiUpdate();
        }

        //public abstract string GetItemRtf();
        //public abstract string GetItemPlainText();
        //public abstract string GetItemQuillHtml();
        //public abstract string[] GetItemFileList();

        public MpContentContainerViewModel() : base() { }

        public MpContentContainerViewModel(MpClipTileViewModel rootTile) : this() {
            HostClipTileViewModel = rootTile;
        }

        public MpContentContainerViewModel(MpClipTileViewModel rootTile,MpCopyItem headItem) : this(rootTile) {
            ItemViewModels = new ObservableCollection<MpContentItemViewModel>() { 
                MpContentItemViewModel.Create(this, headItem) 
            };
            RefreshSubItems();
        }

        #region Commands

        private RelayCommand<object> _toggleEditSubSelectedItemCommand;
        public ICommand ToggleEditSubSelectedItemCommand {
            get {
                if (_toggleEditSubSelectedItemCommand == null) {
                    _toggleEditSubSelectedItemCommand = new RelayCommand<object>(ToggleEditSubSelectedItem, CanToggleEditSubSelectedItem);
                }
                return _toggleEditSubSelectedItemCommand;
            }
        }
        private bool CanToggleEditSubSelectedItem(object args) {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
                   SubSelectedContentItems.Count == 1;
        }
        private void ToggleEditSubSelectedItem(object args) {
            var selectedRtbvm = SubSelectedContentItems[0];
            if (!HostClipTileViewModel.IsEditingTile) {
                HostClipTileViewModel.IsEditingTile = true;
            }
            selectedRtbvm.IsSubSelected = true;
        }

        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if (_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SubSelectedContentItems.Count > 0 &&
                   SubSelectedContentItems.Any(x => VisibleContentItems.IndexOf(x) != VisibleContentItems.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SubSelectedContentItems.Max(x => VisibleContentItems.IndexOf(x));
            ClearSubSelection();
            VisibleContentItems[maxItem + 1].IsSubSelected = true;
        }

        private RelayCommand _selectPreviousItemCommand;
        public ICommand SelectPreviousItemCommand {
            get {
                if (_selectPreviousItemCommand == null) {
                    _selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanSelectPreviousItem);
                }
                return _selectPreviousItemCommand;
            }
        }
        private bool CanSelectPreviousItem() {
            return SubSelectedContentItems.Count > 0 && SubSelectedContentItems.Any(x => VisibleContentItems.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SubSelectedContentItems.Min(x => VisibleContentItems.IndexOf(x));
            ClearSubSelection();
            VisibleContentItems[minItem - 1].IsSubSelected = true;
        }

        private RelayCommand _selectAllCommand;
        public ICommand SelectAllCommand {
            get {
                if (_selectAllCommand == null) {
                    _selectAllCommand = new RelayCommand(SelectAll);
                }
                return _selectAllCommand;
            }
        }
        private void SelectAll() {
            ClearSubSelection();
            foreach (var ctvm in VisibleContentItems) {
                ctvm.IsSubSelected = true;
            }
        }

        private AsyncCommand<Brush> _changeSubSelectedClipsColorCommand;
        public IAsyncCommand<Brush> ChangeSubSelectedClipsColorCommand {
            get {
                if (_changeSubSelectedClipsColorCommand == null) {
                    _changeSubSelectedClipsColorCommand = new AsyncCommand<Brush>(ChangeSubSelectedClipsColor);
                }
                return _changeSubSelectedClipsColorCommand;
            }
        }
        private async Task ChangeSubSelectedClipsColor(Brush brush) {
            if (brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            foreach (var sctvm in SubSelectedContentItems) {
                                sctvm.CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex((brush as SolidColorBrush).Color);
                            }
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private RelayCommand<object> _pasteSubSelectedClipsCommand;
        public ICommand PasteSubSelectedClipsCommand {
            get {
                if (_pasteSubSelectedClipsCommand == null) {
                    _pasteSubSelectedClipsCommand = new RelayCommand<object>(PasteSubSelectedClips, CanPasteSubSelectedClips);
                }
                return _pasteSubSelectedClipsCommand;
            }
        }
        private bool CanPasteSubSelectedClips(object ptapId) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSubSelectedClips(object ptapId) {
            if (ptapId != null && ptapId.GetType() == typeof(int) && (int)ptapId > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.Where(x => x.PasteToAppPathId == (int)ptapId).ToList()[0];
            } else if (ptapId != null && ptapId.GetType() == typeof(IntPtr) && (IntPtr)ptapId != IntPtr.Zero) {
                //when pasting to a running application
                _selectedPasteToAppPathWindowHandle = (IntPtr)ptapId;
                _selectedPasteToAppPathViewModel = null;
            } else {
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = null;
            }
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items
            MainWindowViewModel.HideWindowCommand.Execute(true);
        }

        private AsyncCommand _bringSubSelectedClipTilesToFrontCommand;
        public IAsyncCommand BringSubSelectedClipTilesToFrontCommand {
            get {
                if (_bringSubSelectedClipTilesToFrontCommand == null) {
                    _bringSubSelectedClipTilesToFrontCommand = new AsyncCommand(BringSubSelectedClipTilesToFront, CanBringSubSelectedClipTilesToFront);
                }
                return _bringSubSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSubSelectedClipTilesToFront(object arg) {
            if (IsBusy || MpMainWindowViewModel.IsMainWindowLoading || VisibleContentItems.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SubSelectedContentItems.Count && i < VisibleContentItems.Count; i++) {
                if (!SubSelectedContentItems.Contains(VisibleContentItems[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private async Task BringSubSelectedClipTilesToFront() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedContentItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                ItemViewModels.Move(ItemViewModels.IndexOf(sctvm), 0);
                                sctvm.IsSubSelected = true;
                            }
                            RequestScrollIntoView(SubSelectedContentItems[0]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private AsyncCommand _sendSubSelectedClipTilesToBackCommand;
        public IAsyncCommand SendSubSelectedClipTilesToBackCommand {
            get {
                if (_sendSubSelectedClipTilesToBackCommand == null) {
                    _sendSubSelectedClipTilesToBackCommand = new AsyncCommand(SendSubSelectedClipTilesToBack, CanSendSubSelectedClipTilesToBack);
                }
                return _sendSubSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSubSelectedClipTilesToBack(object args) {
            if (IsBusy || MpMainWindowViewModel.IsMainWindowLoading || VisibleContentItems.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SubSelectedContentItems.Count && i < VisibleContentItems.Count; i++) {
                if (!SubSelectedContentItems.Contains(VisibleContentItems[VisibleContentItems.Count - 1 - i])) {
                    canSendBack = true;
                    break;
                }
            }
            return canSendBack;
        }
        private async Task SendSubSelectedClipTilesToBack() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedContentItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                ItemViewModels.Move(ItemViewModels.IndexOf(sctvm), ItemViewModels.Count - 1);
                                sctvm.IsSubSelected = true;
                            }
                            RequestScrollIntoView(SubSelectedContentItems[SubSelectedContentItems.Count - 1]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }


        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(HeadItem.CopyItem.ItemData.ToPlainText()));
        }

        private RelayCommand _deleteSubSelectedClipsCommand;
        public ICommand DeleteSubSelectedClipsCommand {
            get {
                if (_deleteSubSelectedClipsCommand == null) {
                    _deleteSubSelectedClipsCommand = new RelayCommand(DeleteSubSelectedClips, CanDeleteSubSelectedClips);
                }
                return _deleteSubSelectedClipsCommand;
            }
        }
        private bool CanDeleteSubSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate;
        }
        private void DeleteSubSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SubSelectedContentItems) {
                lastSelectedClipTileIdx = VisibleContentItems.IndexOf(ct);
                ItemViewModels.Remove(ct);
            }
            ClearSubSelection();
            if (VisibleContentItems.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibleContentItems[0].IsSubSelected = true;
                } else if (lastSelectedClipTileIdx < VisibleContentItems.Count) {
                    VisibleContentItems[lastSelectedClipTileIdx].IsSubSelected = true;
                } else {
                    VisibleContentItems[lastSelectedClipTileIdx - 1].IsSubSelected = true;
                }
            }
        }

        private RelayCommand<MpTagTileViewModel> _linkTagToSubSelectedClipsCommand;
        public ICommand LinkTagToSubSelectedClipsCommand {
            get {
                if (_linkTagToSubSelectedClipsCommand == null) {
                    _linkTagToSubSelectedClipsCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToSubSelectedClips, CanLinkTagToSubSelectedClips);
                }
                return _linkTagToSubSelectedClipsCommand;
            }
        }
        private bool CanLinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if (tagToLink == null || SubSelectedContentItems == null || SubSelectedContentItems.Count == 0) {
                return false;
            }
            if (SubSelectedContentItems.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinked(SubSelectedContentItems[0].CopyItem);
            foreach (var srtbvm in SubSelectedContentItems) {
                if (tagToLink.IsLinked(srtbvm) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinked(SubSelectedContentItems[0].CopyItem);
            foreach (var srtbvm in SubSelectedContentItems) {
                if (isUnlink) {
                    tagToLink.RemoveClip(srtbvm);
                } else {
                    tagToLink.AddClip(srtbvm);
                }
            }
            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();
            MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey, CanAssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private bool CanAssignHotkey() {
            return SubSelectedContentItems.Count == 1;
        }
        private void AssignHotkey() {
            SubSelectedContentItems[0].AssignHotkeyCommand.Execute(null);
        }

        private RelayCommand _invertSubSelectionCommand;
        public ICommand InvertSubSelectionCommand {
            get {
                if (_invertSubSelectionCommand == null) {
                    _invertSubSelectionCommand = new RelayCommand(InvertSubSelection, CanSubInvertSelection);
                }
                return _invertSubSelectionCommand;
            }
        }
        private bool CanSubInvertSelection() {
            return SubSelectedContentItems.Count != VisibleContentItems.Count;
        }
        private void InvertSubSelection() {
            var sctvml = SubSelectedContentItems;
            ClearSubSelection();
            foreach (var vctvm in VisibleContentItems) {
                if (!sctvml.Contains(vctvm)) {
                    vctvm.IsSubSelected = true;
                }
            }
        }

        private AsyncCommand _speakSubSelectedClipsAsyncCommand;
        public IAsyncCommand SpeakSubSelectedClipsAsyncCommand {
            get {
                if (_speakSubSelectedClipsAsyncCommand == null) {
                    _speakSubSelectedClipsAsyncCommand = new AsyncCommand(SpeakSubSelectedClipsAsync, CanSpeakSubSelectedClipsAsync);
                }
                return _speakSubSelectedClipsAsyncCommand;
            }
        }
        private bool CanSpeakSubSelectedClipsAsync(object args) {
            foreach (var sctvm in SubSelectedContentItems) {
                if (!string.IsNullOrEmpty(sctvm.CopyItem.ItemData.ToPlainText())) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSubSelectedClipsAsync() {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                var speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                string voiceName = speechSynthesizer.GetInstalledVoices()[3].VoiceInfo.Name;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName)) {
                    var voice = speechSynthesizer.GetInstalledVoices().Where(x => x.VoiceInfo.Name.ToLower().Contains(Properties.Settings.Default.SpeechSynthVoiceName.ToLower())).FirstOrDefault();
                    if (voice != null) {
                        voiceName = voice.VoiceInfo.Name;
                    }
                }
                speechSynthesizer.SelectVoice(voiceName);

                speechSynthesizer.Rate = 0;
                speechSynthesizer.SpeakCompleted += (s, e) => {
                    speechSynthesizer.Dispose();
                };
                // Create a PromptBuilder object and append a text string.
                PromptBuilder promptBuilder = new PromptBuilder();

                foreach (var sctvm in SubSelectedContentItems) {
                    //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                    promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItem.ItemData.ToPlainText());
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);

            }, DispatcherPriority.Background);
        }

        private RelayCommand _duplicateSubSelectedClipsCommand;
        public ICommand DuplicateSubSelectedClipsCommand {
            get {
                if (_duplicateSubSelectedClipsCommand == null) {
                    _duplicateSubSelectedClipsCommand = new RelayCommand(DuplicateSubSelectedClips);
                }
                return _duplicateSubSelectedClipsCommand;
            }
        }
        private void DuplicateSubSelectedClips() {
            var tempSubSelectedRtbvml = SubSelectedContentItems;
            ClearSubSelection();
            foreach (var srtbvm in tempSubSelectedRtbvml) {
                var clonedCopyItem = (MpCopyItem)srtbvm.CopyItem.Clone();
                clonedCopyItem.WriteToDatabase();
                var rtbvm = new MpRtbItemViewModel(this, clonedCopyItem);
                //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                ItemViewModels.Add(rtbvm);
                rtbvm.IsSubSelected = true;
            }
        }
        #endregion
    }
}
