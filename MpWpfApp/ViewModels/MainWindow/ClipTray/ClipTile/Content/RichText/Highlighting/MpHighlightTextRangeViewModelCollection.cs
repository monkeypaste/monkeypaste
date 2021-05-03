using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpHighlightTextRangeViewModelCollection : MpObservableCollectionViewModel<MpHighlightTextRangeViewModel>, IDisposable {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        public MpHighlightTextRangeViewModel SelectedHighlightTextRangeViewModel {
            get {
                foreach (var hltrvm in this) {
                    if (hltrvm.IsSelected) {
                        return hltrvm;
                    }
                }
                return null;
            }
        }
        #endregion

        public List<KeyValuePair<TextRange, Brush>> NonTransparentDocumentBackgroundRangeList { get; set; } = new List<KeyValuePair<TextRange, Brush>>();
        
        private int _highlightTaskCount = 0;
        public int HighlightTaskCount {
            get {
                return _highlightTaskCount;
            }
            set {
                if (_highlightTaskCount != value) {
                    _highlightTaskCount = value;
                    OnPropertyChanged(nameof(HighlightTaskCount));
                }
            }
        }

        public bool HasAppMatch {
            get {
                foreach (var htrvm in this) {
                    if (htrvm.HighlightType == MpHighlightType.App) {
                        return true;
                    }
                }
                return false;
            }
        }

        public List<MpRtbListBoxItemRichTextBoxViewModel> AppMatchRtbvmList {
            get {
                var appMatchRtbvmList = new List<MpRtbListBoxItemRichTextBoxViewModel>();
                foreach (var htrvm in this) {
                    if (htrvm.HighlightType == MpHighlightType.App && 
                        htrvm.RtbItemViewModel != null) {
                        appMatchRtbvmList.Add(htrvm.RtbItemViewModel);
                    }
                }
                return appMatchRtbvmList;
            }
        }

        public Dictionary<object,Visibility> VisibilityDictionary {
            get {
                var vdict = new Dictionary<object, Visibility>();
                if (this.Count == 0) {
                    if(MainWindowViewModel.SearchBoxViewModel.HasText) {
                        vdict.Add(ClipTileViewModel, Visibility.Collapsed);
                    } else {
                        vdict.Add(ClipTileViewModel, Visibility.Visible);
                        foreach(var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                            vdict.Add(rtbvm, Visibility.Visible);
                        }
                    }
                } else {
                    foreach(var hltrvm in this) {
                        if(hltrvm.RtbItemViewModel == null) {
                            if(!vdict.ContainsKey(ClipTileViewModel)) {
                                vdict.Add(ClipTileViewModel, Visibility.Visible);
                            }                            
                        } else {
                            if(!vdict.ContainsKey(hltrvm.RtbItemViewModel)) {
                                vdict.Add(hltrvm.RtbItemViewModel, Visibility.Visible);
                            }                            
                        }
                    }
                    if(vdict.Count > 0 && !vdict.ContainsKey(ClipTileViewModel)) {
                        vdict.Add(ClipTileViewModel, Visibility.Visible);
                    } else if(!vdict.ContainsKey(ClipTileViewModel)) {
                        vdict.Add(ClipTileViewModel, Visibility.Collapsed);
                    }
                    foreach(var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                        //this loop adds any unmatched rtbvm's to the dictionary so they are collapsed
                        if(!vdict.ContainsKey(rtbvm)) {
                            vdict.Add(rtbvm, Visibility.Collapsed);
                        }
                    }
                }
                return vdict;
            }
        }
        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModelCollection() : this(null) {}

        public MpHighlightTextRangeViewModelCollection(MpClipTileViewModel ctvm) : base() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(SelectedHighlightTextRangeViewModel):
                        ApplyHighlightingCommand.Execute(null);
                        break;
                    case nameof(HighlightTaskCount):
                        MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsSearching = HighlightTaskCount > 0;
                        if (HighlightTaskCount < 0) {
                            HighlightTaskCount = 0;
                        }
                        break;
                }
            };

            CollectionChanged += (s, e) => {
                //this.Sort(x => x);
            };
            ClipTileViewModel = ctvm;            
        }

        public void UpdateInDocumentsBgColorList(RichTextBox rtb) {
            var colorRangesToRemove = new List<KeyValuePair<TextRange, Brush>>();
            foreach(var kvp in NonTransparentDocumentBackgroundRangeList) {
                if(kvp.Key.Start.IsInSameDocument(rtb.Document.ContentStart)) {
                    colorRangesToRemove.Add(kvp);
                }
            }
            foreach(var kvpToRemove in colorRangesToRemove) {
                NonTransparentDocumentBackgroundRangeList.Remove(kvpToRemove);
            }

            NonTransparentDocumentBackgroundRangeList.AddRange(rtb.FindNonTransparentRangeList());
        }

        public async Task<Dictionary<object,Visibility>> PerformHighlightingAsync(string hlt) {
            HighlightTaskCount++;

            ClearHighlightingCommand.Execute(null);

            if (MpMainWindowViewModel.IsApplicationLoading || ClipTileViewModel.IsLoading) {
                HighlightTaskCount--;
                return VisibilityDictionary;
            }
            
            if(MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded && !ClipTileViewModel.IsExpanded) {
                HighlightTaskCount--;
                return new Dictionary<object, Visibility> { { ClipTileViewModel, Visibility.Collapsed } };
            }

            var sttvm = MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
            if (!sttvm.IsLinkedWithClipTile(ClipTileViewModel)) {
                Console.WriteLine("Clip tile w/ title " + ClipTileViewModel.CopyItemTitle + " is not linked with current tag");
                HighlightTaskCount--;
                return new Dictionary<object, Visibility> { { ClipTileViewModel, Visibility.Collapsed } };
            }

            if (string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                ClearHighlightingCommand.Execute(null);
                HighlightTaskCount--;

                ClipTileViewModel.ResetContentScroll();
                return VisibilityDictionary;
            }

            var result = await PerformHighlightAsync(hlt);
            HighlightTaskCount--;

            return result;
        }
        
        private async Task<Dictionary<object,Visibility>> PerformHighlightAsync(string hlt) {
            var result = Visibility.Visible;
            await Dispatcher.CurrentDispatcher.InvokeAsync(
                (Action)(() => {
                    var ttb = ClipTileViewModel.TitleTextBlock;
                    int sortIdx = 0;

                    #region Pre-Pass Checks
                    bool tc = false, cc = false, anc = false, apc = false, uc = false;

                    if(Properties.Settings.Default.SearchByTitle) {
                        tc = ClipTileViewModel.CopyItemTitle.ContainsByCaseSetting(hlt);
                        if (ClipTileViewModel.IsTextItem && !tc) {
                            foreach (var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                tc = rtbvm.CopyItemTitle.ContainsByCaseSetting(hlt) ? true : tc;
                            }
                        }
                    }                    
                    if(Properties.Settings.Default.SearchByRichText) {
                        cc = ClipTileViewModel.CopyItemPlainText.ContainsByCaseSetting(hlt) ||
                             ClipTileViewModel.CopyItemDescription.ContainsByCaseSetting(hlt);
                        if (ClipTileViewModel.IsTextItem && !cc) {
                            foreach (var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                cc = rtbvm.CopyItemPlainText.ContainsByCaseSetting(hlt) ||
                                     ClipTileViewModel.CopyItemDescription.ContainsByCaseSetting(hlt) ? true : cc;
                            }
                        }
                    }

                    if (Properties.Settings.Default.SearchByApplicationName) {
                        anc = ClipTileViewModel.CopyItemAppName.ContainsByCaseSetting(hlt);
                        if (anc) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, null, sortIdx++, MpHighlightType.App));                            
                        }
                        if (ClipTileViewModel.IsTextItem) {
                            foreach (var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                bool ranc = rtbvm.CopyItemAppName.ContainsByCaseSetting(hlt);
                                anc = ranc ? true : anc;
                                if(ranc) {
                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, rtbvm, null, sortIdx++, MpHighlightType.App));
                                }
                            }
                        }
                    }
                    if (Properties.Settings.Default.SearchByProcessName) {
                        apc = ClipTileViewModel.CopyItemAppPath.ContainsByCaseSetting(hlt);
                        if (apc) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, null, sortIdx++, MpHighlightType.App));
                        }
                        if (ClipTileViewModel.IsTextItem) {
                            foreach (var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                bool rapc = rtbvm.CopyItemAppPath.ContainsByCaseSetting(hlt);
                                apc = rapc ? true : apc;
                                if(rapc) {
                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, rtbvm, null, sortIdx++, MpHighlightType.App));
                                }
                            }
                        }
                    }

                    if (Properties.Settings.Default.SearchBySourceUrl) {
                        uc = ClipTileViewModel.CopyItemUrl.UrlPath.ContainsByCaseSetting(hlt);
                        if (uc) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, null, sortIdx++, MpHighlightType.App));
                        }
                        if (ClipTileViewModel.IsTextItem) {
                            foreach (var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                bool ruc = rtbvm.CopyItemUrl.UrlPath.ContainsByCaseSetting(hlt);
                                uc = ruc ? true : uc;
                                if(ruc) {
                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, rtbvm, null, sortIdx++, MpHighlightType.App));
                                }
                            }
                        }
                    }

                    if (!tc && !cc && !anc && !apc && !uc) {                        
                        result = Visibility.Collapsed;
                        return;
                    }
                    #endregion

                    //begin text range highlighting

                    Console.WriteLine("Beginning highlight clip with title: " + ClipTileViewModel.CopyItemTitle + " with highlight text: " + hlt);
                    
                    //highlight title 
                    if(tc && ttb != null) {
                        var trl = MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                        foreach (var mr in trl) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, mr, sortIdx++, MpHighlightType.Title));
                        }
                        if (ClipTileViewModel.IsTextItem) {
                            foreach(var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                                var strl = MpHelpers.Instance.FindStringRangesFromPosition(rtbvm.RtbListBoxItemTitleTextBlock.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                foreach (var mr in strl) {
                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, rtbvm, mr, sortIdx++, MpHighlightType.Title));
                                }
                            }
                        }
                    }
                    switch (ClipTileViewModel.CopyItemType) {
                        case MpCopyItemType.RichText:
                        case MpCopyItemType.Composite:                            
                            if (cc) {
                                foreach(var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {                             
                                    var rtbvmtrl = MpHelpers.Instance.FindStringRangesFromPosition(rtbvm.Rtb.Document.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                    foreach (var mr in rtbvmtrl) {
                                        this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, rtbvm, mr, sortIdx++,MpHighlightType.Text));
                                    }
                                }
                            }
                            break;
                        case MpCopyItemType.Image:
                            if(Properties.Settings.Default.SearchByImage && cc) {
                                //already found in pre-pass
                                this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, null, -1, MpHighlightType.Image));
                            }
                            break;
                        case MpCopyItemType.FileList:
                            if(Properties.Settings.Default.SearchByFileList && cc) {
                                var flb = ClipTileViewModel.FileListBox;
                                foreach (var fivm in ClipTileViewModel.FileListCollectionViewModel) {                                    
                                    if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                        var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                        if (container != null) {
                                            var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                            if (fitb != null) {
                                                var hlrl = MpHelpers.Instance.FindStringRangesFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                                foreach (var mr in hlrl) {
                                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel,null, mr, sortIdx++,MpHighlightType.Text));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    Console.WriteLine("Ending highlighting clip with title: " + ClipTileViewModel.CopyItemTitle);
                    ResetSelection();
                    result = this.Count > 0 ? Visibility.Visible:Visibility.Collapsed;
                }),
                DispatcherPriority.Background);

            return VisibilityDictionary;
        }

        public void ClearSelection() {
            foreach(var htrvm in this) {
                htrvm.IsSelected = false;
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(this.Count > 0) {
                this[0].IsSelected = true;
                OnPropertyChanged(nameof(SelectedHighlightTextRangeViewModel));
            }
        }

        public new void Dispose() {
            base.Dispose();
            this.Clear();
        }

        public new void Add(MpHighlightTextRangeViewModel htrvm) {
            if(htrvm.HighlightType != MpHighlightType.App) {
                base.Add(htrvm);
                return;
            }
            bool alreadyHasAppHighlight = this.Any(x => x.HighlightType == MpHighlightType.App && x.RtbItemViewModel == htrvm.RtbItemViewModel);
            if(!alreadyHasAppHighlight) {
                base.Add(htrvm);
            }
        }
        #endregion

        #region Private Methods
        private Dictionary<object,Visibility> AddSubResult(Dictionary<object,Visibility> dict, object vm, Visibility vis) {
            //this abstracts adding sub results so if dict contains a collapsed kvp it is removed if new one is visible
            //and ignores if new one exists or simply adds it
            if(dict.ContainsKey(vm)) {
                if(dict[vm] == vis) {
                    return dict;
                }
                if(dict[vm] == Visibility.Collapsed && vis == Visibility.Visible) {
                    dict.Remove(vm);
                    dict.Add(vm, vis);
                }
            } else {
                dict.Add(vm, vis);
            }
            return dict;
        }
        public int CompareHighlightRanges(MpHighlightTextRangeViewModel a,MpHighlightTextRangeViewModel b) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj
            if (a == null) {
                if(b == null) {
                    return 0;
                }
                return 1;
            }
            if(b == null) {
                return -1;
            }
            //if (Range.Start == ohltrvm.Range.Start && Range.End == ohltrvm.Range.End) {
            //    return 0;
            //}
            if (a.SortOrderIdx < b.SortOrderIdx) {
                return -1;
            }
            if (a.SortOrderIdx > b.SortOrderIdx) {
                return 1;
            }
            if (!a.Range.Start.IsInSameDocument(b.Range.Start)) {
                return -1;
            }
            return a.Range.Start.CompareTo(b.Range.Start);
            //return ohltrvm.Range.Start.CompareTo(Range.Start);
        }

        private void ReplaceDocumentsBgColors() {
            foreach(var kvp in NonTransparentDocumentBackgroundRangeList) {
                kvp.Key.ApplyPropertyValue(TextElement.BackgroundProperty, kvp.Value);
            }
        }
        #endregion

        #region Commands
        private RelayCommand _selectNextMatchCommand;
        public ICommand SelectNextMatchCommand {
            get {
                if(_selectNextMatchCommand == null) {
                    _selectNextMatchCommand = new RelayCommand(SelectNextMatch, CanSelectNextMatch);
                }
                return _selectNextMatchCommand;
            }
        }
        private bool CanSelectNextMatch() {
            return this.Count > 0;
        }
        private void SelectNextMatch() {
            int curIdx = this.IndexOf(SelectedHighlightTextRangeViewModel);
            int nextIdx = curIdx + 1;
            if (nextIdx >= this.Count) {
                nextIdx = 0;
            }
            Console.WriteLine("CurIdx: " + curIdx + " NextIdx: " + nextIdx);
            ClearSelection();
            this[nextIdx].IsSelected = true;
            OnPropertyChanged(nameof(SelectedHighlightTextRangeViewModel));
        }

        private RelayCommand _selectPreviousMatchCommand;
        public ICommand SelectPreviousMatchCommand {
            get {
                if (_selectPreviousMatchCommand == null) {
                    _selectPreviousMatchCommand = new RelayCommand(SelectPreviousMatch, CanSelectPreviousMatch);
                }
                return _selectPreviousMatchCommand;
            }
        }
        private bool CanSelectPreviousMatch() {
            return this.Count > 0;
        }
        private void SelectPreviousMatch() {
            int prevIdx = this.IndexOf(SelectedHighlightTextRangeViewModel) - 1;
            if (prevIdx < 0) {
                prevIdx = this.Count - 1;
            }
            ClearSelection();
            this[prevIdx].IsSelected = true;
            OnPropertyChanged(nameof(SelectedHighlightTextRangeViewModel));
        }

        private RelayCommand<object> _applyHighlightingCommand;
        public ICommand ApplyHighlightingCommand {
            get {
                if(_applyHighlightingCommand == null) {
                    _applyHighlightingCommand = new RelayCommand<object>(ApplyHighlighting);
                }
                return _applyHighlightingCommand;
            }
        }
        private void ApplyHighlighting(object args) {
            foreach(var hltrvm in this) {
                if (args == null || hltrvm == args) {
                    hltrvm.HighlightRange();
                }
            }
        }

        private RelayCommand<object> _hideHighlightingCommand;
        public ICommand HideHighlightingCommand {
            get {
                if (_hideHighlightingCommand == null) {
                    _hideHighlightingCommand = new RelayCommand<object>(HideHighlighting);
                }
                return _hideHighlightingCommand;
            }
        }
        private void HideHighlighting(object args) {
            foreach (var hltrvm in this) {
                if (args == null || hltrvm == args) {
                    hltrvm.ClearHighlighting();
                }                
            }
            ReplaceDocumentsBgColors();
        }

        private RelayCommand _clearHighlightingCommand;
        public ICommand ClearHighlightingCommand {
            get {
                if (_clearHighlightingCommand == null) {
                    _clearHighlightingCommand = new RelayCommand(ClearHighlighting);
                }
                return _clearHighlightingCommand;
            }
        }
        private void ClearHighlighting() {
            HideHighlightingCommand.Execute(null);
            this.Clear();
            ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.AppIconHighlightBorderVisibility));
        }        
        #endregion
    }
}
