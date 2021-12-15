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
using MonkeyPaste;
using System.Collections.ObjectModel;

namespace MpWpfApp {
    public class MpHighlightTextRangeViewModelCollection : MpViewModelBase<MpClipTileViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpHighlightTextRangeViewModel SelectedItem {
            get {
                foreach (var hltrvm in Items) {
                    if (hltrvm.IsSelected) {
                        return hltrvm;
                    }
                }
                return null;
            }
        }

        public ObservableCollection<MpHighlightTextRangeViewModel> Items { get; private set; } = new ObservableCollection<MpHighlightTextRangeViewModel>();
        #endregion

        public int Count {
            get {
                return Items.Count;
            }
        }

        public List<KeyValuePair<TextRange, Brush>> NonTransparentDocumentBackgroundRangeList { get; set; } = new List<KeyValuePair<TextRange, Brush>>();
        
        private int _highlightTaskCount = 0;
        public int HighlightTaskCount { get; set; }

        public bool HasAppMatch => Items.Any(x => x.HighlightType == MpHighlightType.App);

        public List<MpContentItemViewModel> AppItems => Items.Where(x => x.HighlightType == MpHighlightType.App).Select(x => x.ContentItemViewModel).ToList();

        //public Dictionary<object,Visibility> VisibilityDictionary {
        //    get {
        //        var vdict = new Dictionary<object, Visibility>();
        //        if (Items.Count == 0) {
        //            if(MpSearchBoxViewModel.Instance.HasText) {
        //                vdict.Add(Parent, Visibility.Collapsed);
        //            } else {
        //                vdict.Add(Parent, Visibility.Visible);
        //                foreach(var rtbvm in base.Parent.ItemViewModels) {
        //                    vdict.Add(rtbvm, Visibility.Visible);
        //                }
        //            }
        //        } else {
        //            foreach(var hltrvm in Items) {
        //                if(hltrvm.ContentItemViewModel == null) {
        //                    if(!vdict.ContainsKey(Parent)) {
        //                        vdict.Add(Parent, Visibility.Visible);
        //                    }                            
        //                } else {
        //                    if(!vdict.ContainsKey(hltrvm.ContentItemViewModel)) {
        //                        vdict.Add(hltrvm.ContentItemViewModel, Visibility.Visible);
        //                    }                            
        //                }
        //            }
        //            if(vdict.Count > 0 && !vdict.ContainsKey(Parent)) {
        //                vdict.Add(Parent, Visibility.Visible);
        //            } else if(!vdict.ContainsKey(Parent)) {
        //                vdict.Add(Parent, Visibility.Collapsed);
        //            }
        //            foreach(var rtbvm in base.Parent.ItemViewModels) {
        //                //this loop adds any unmatched rtbvm's to the dictionary so they are collapsed
        //                if(!vdict.ContainsKey(rtbvm)) {
        //                    vdict.Add(rtbvm, Visibility.Collapsed);
        //                }
        //            }
        //        }
        //        return vdict;
        //    }
        //}
        #endregion

        #region Events
        public event EventHandler<object> OnHighlightItemSelectionChanged;
        #endregion

        #region Public Methods
        public MpHighlightTextRangeViewModelCollection() : base(null) {}

        public MpHighlightTextRangeViewModelCollection(MpClipTileViewModel parent) : base(parent) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(SelectedItem):
                        OnHighlightItemSelectionChanged?.Invoke(this, SelectedItem);
                        ApplyHighlightingCommand.Execute(null);
                        break;
                    case nameof(HighlightTaskCount):
                        MpSearchBoxViewModel.Instance.IsSearching = HighlightTaskCount > 0;
                        if (HighlightTaskCount < 0) {
                            HighlightTaskCount = 0;
                        }
                        break;

                }
            };           
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

        public async Task<Dictionary<object,Visibility>> PerformHighlightingAsync(string hlt, List<Tuple<TextBlock, RichTextBox>> tl) {
            HighlightTaskCount++;

            ClearHighlightingCommand.Execute(null);

            if (MpMainWindowViewModel.Instance.IsMainWindowLoading || Parent.IsLoading) {
                HighlightTaskCount--;
                return VisibilityDictionary;
            }
            
            if(MpClipTrayViewModel.Instance.IsAnyTileExpanded && !Parent.IsExpanded) {
                HighlightTaskCount--;
                return new Dictionary<object, Visibility> { { Parent, Visibility.Collapsed } };
            }

            var sttvm = MpTagTrayViewModel.Instance.SelectedTagTile;
            bool isLinked = await sttvm.IsLinked(Parent);
            if (!isLinked) {
                //MonkeyPaste.MpConsole.WriteLine("Clip tile w/ title " + HostClipTileViewModel.CopyItemTitle + " is not linked with current tag");
                HighlightTaskCount--;
                return new Dictionary<object, Visibility> { { Parent, Visibility.Collapsed } };
            }

            if (string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                ClearHighlightingCommand.Execute(null);
                HighlightTaskCount--;

                Parent.ResetContentScroll();
                return VisibilityDictionary;
            }

            var result = await PerformHighlightAsyncHelper(hlt,tl);
            HighlightTaskCount--;

            return result;
        }
        
        private async Task<Dictionary<object,Visibility>> PerformHighlightAsyncHelper(string hlt, List<Tuple<TextBlock,RichTextBox>> tl) {
            var result = Visibility.Visible;
            await Dispatcher.CurrentDispatcher.InvokeAsync(
                (Action)(() => {
                    int sortIdx = 0;

                    #region Pre-Pass Checks
                    bool tc = false, cc = false, anc = false, apc = false, uc = false;

                    if(Properties.Settings.Default.SearchByTitle) {
                        foreach (var rtbvm in base.Parent.ItemViewModels) {
                            tc = rtbvm.CopyItem.Title.ContainsByCaseSetting(hlt) ? true : tc;
                        }
                    }                    
                    if(Properties.Settings.Default.SearchByRichText) {
                        foreach (var rtbvm in base.Parent.ItemViewModels) {
                            cc = rtbvm.CopyItem.ItemData.ContainsByCaseSetting(hlt) ? true : cc;
                        }
                    }

                    if (Properties.Settings.Default.SearchByApplicationName) {
                        foreach (var rtbvm in base.Parent.ItemViewModels) {
                            bool ranc = rtbvm.CopyItem.Source.App.AppName.ContainsByCaseSetting(hlt);
                            anc = ranc ? true : anc;
                            if (ranc) {
                                this.Add(new MpHighlightTextRangeViewModel(this, Parent, rtbvm, null, sortIdx++, MpHighlightType.App));
                            }
                        }
                    }
                    if (Properties.Settings.Default.SearchByProcessName) {
                        foreach (var rtbvm in base.Parent.ItemViewModels) {
                            bool rapc = rtbvm.CopyItem.Source.App.SourcePath.ContainsByCaseSetting(hlt);
                            apc = rapc ? true : apc;
                            if (rapc) {
                                this.Add(new MpHighlightTextRangeViewModel(this, Parent, rtbvm, null, sortIdx++, MpHighlightType.App));
                            }
                        }
                    }

                    if (Properties.Settings.Default.SearchBySourceUrl && base.Parent.ItemViewModels.Any(x => x.CopyItem.Source.Url != null)) {
                        foreach (var rtbvm in base.Parent.ItemViewModels) {
                            bool ruc = rtbvm.CopyItem.Source.Url.UrlPath.ContainsByCaseSetting(hlt);
                            uc = ruc ? true : uc;
                            if (ruc) {
                                this.Add(new MpHighlightTextRangeViewModel(this, Parent, rtbvm, null, sortIdx++, MpHighlightType.App));
                            }
                        }
                    }

                    if (!tc && !cc && !anc && !apc && !uc) {                        
                        result = Visibility.Collapsed;
                        return;
                    }
                    #endregion

                    //begin text range highlighting

                    //MonkeyPaste.MpConsole.WriteLine("Beginning highlight clip with title: " + ClipTileViewModel.CopyItemTitle + " with highlight text: " + hlt);
                    
                    //highlight title 
                    if(tc) {
                        foreach (var t in tl) {
                            var ttb = t.Item1;
                            var rtbvm = t.Item2.DataContext as MpContentItemViewModel;
                            var strl = MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                            foreach (var mr in strl) {
                                this.Add(new MpHighlightTextRangeViewModel(this, Parent, rtbvm, mr, sortIdx++, MpHighlightType.Title));
                            }
                        }
                    }
                    foreach(var civm in base.Parent.ItemViewModels) {
                        switch (civm.CopyItem.ItemType) {
                            case MpCopyItemType.RichText:
                                if (cc) {
                                    foreach (var t in tl) {
                                        var rtb = t.Item2;
                                        var rtbvm = rtb.DataContext as MpContentItemViewModel;
                                        var rtbvmtrl = MpHelpers.Instance.FindStringRangesFromPosition(rtb.Document.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                        foreach (var mr in rtbvmtrl) {
                                            this.Add(new MpHighlightTextRangeViewModel(this, Parent, rtbvm, mr, sortIdx++, MpHighlightType.Text));
                                        }
                                    }
                                }
                                break;
                            case MpCopyItemType.Image:
                                if (Properties.Settings.Default.SearchByImage && cc) {
                                    //already found in pre-pass
                                    this.Add(new MpHighlightTextRangeViewModel(this, Parent, null, null, -1, MpHighlightType.Image));
                                }
                                break;
                                //case MpCopyItemType.FileList:
                                //    if(Properties.Settings.Default.SearchByFileList && cc) {
                                //        var flb = HostClipTileViewModel.FileListBox;
                                //        foreach (var fivm in HostClipTileViewModel.FileListCollectionViewModel) {                                    
                                //            if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                //                var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                //                if (container != null) {
                                //                    var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                //                    if (fitb != null) {
                                //                        var hlrl = MpHelpers.Instance.FindStringRangesFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                //                        foreach (var mr in hlrl) {
                                //                            this.Add(new MpHighlightTextRangeViewModel(this,HostClipTileViewModel,null, mr, sortIdx++,MpHighlightType.Text));
                                //                        }
                                //                    }
                                //                }
                                //            }
                                //        }
                                //    }
                                //    break;
                        }
                    }
                   // MonkeyPaste.MpConsole.WriteLine("Ending highlighting clip with title: " + HostClipTileViewModel.CopyItemTitle);
                    ResetSelection();
                    result = Items.Count > 0 ? Visibility.Visible:Visibility.Collapsed;
                }),
                DispatcherPriority.Background);

            return VisibilityDictionary;
        }

        public void ClearSelection() {
            foreach(var htrvm in Items) {
                htrvm.IsSelected = false;
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(Items.Count > 0) {
                Items[0].IsSelected = true;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public override void Dispose() {
            base.Dispose();
            Items.Clear();
        }

        public void Add(MpHighlightTextRangeViewModel htrvm) {
            if(htrvm.HighlightType != MpHighlightType.App) {
                Items.Add(htrvm);
                return;
            }
            bool alreadyHasAppHighlight = Items.Any(x => x.HighlightType == MpHighlightType.App && x.ContentItemViewModel == htrvm.ContentItemViewModel);
            if(!alreadyHasAppHighlight) {
                Items.Add(htrvm);
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
            int curIdx = Items.IndexOf(SelectedItem);
            int nextIdx = curIdx + 1;
            if (nextIdx >= this.Count) {
                nextIdx = 0;
            }
            MonkeyPaste.MpConsole.WriteLine("CurIdx: " + curIdx + " NextIdx: " + nextIdx);
            ClearSelection();
            Items[nextIdx].IsSelected = true;
            OnPropertyChanged(nameof(SelectedItem));
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
            int prevIdx = Items.IndexOf(SelectedItem) - 1;
            if (prevIdx < 0) {
                prevIdx = this.Count - 1;
            }
            ClearSelection();
            Items[prevIdx].IsSelected = true;
            OnPropertyChanged(nameof(SelectedItem));
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
            foreach(var hltrvm in Items) {
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
            foreach (var hltrvm in Items) {
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
            Items.Clear();
            Parent?.OnPropertyChanged(nameof(Parent.AppIconHighlightBorderVisibility));
        }        
        #endregion
    }
}
