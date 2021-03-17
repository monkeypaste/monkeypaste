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

        #region View Models
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
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

        #region Properties
        public List<KeyValuePair<TextRange, Brush>> NonTransparentDocumentBackgroundRangeList = new List<KeyValuePair<TextRange, Brush>>();
        
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
                foreach(var htrvm in this) {
                    if(htrvm.IsAppRange) {
                        return true;
                    }
                }
                return false;
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

        public async Task<Visibility> PerformHighlightingAsync(string hlt) {
            HighlightTaskCount++;

            ClearHighlightingCommand.Execute(null);

            if (ClipTileViewModel.MainWindowViewModel.IsLoading || ClipTileViewModel.IsLoading) {
                HighlightTaskCount--;
                return Visibility.Visible;
            }
            
            if(MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded && !ClipTileViewModel.IsExpanded) {
                HighlightTaskCount--;
                return Visibility.Collapsed;
            }

            var sttvm = MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
            if (!sttvm.IsLinkedWithClipTile(ClipTileViewModel)) {
                Console.WriteLine("Clip tile w/ title " + ClipTileViewModel.CopyItemTitle + " is not linked with current tag");
                HighlightTaskCount--;
                return Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                ClearHighlightingCommand.Execute(null);
                HighlightTaskCount--;

                ClipTileViewModel.ResetContentScroll();
                return Visibility.Visible;
            }

            var result = await PerformHighlightAsync(hlt);
            HighlightTaskCount--;
            //var thisAsList = this.ToList();
            //thisAsList.Sort(CompareHighlightRanges);
            //this.Clear();
            //foreach(var hltrvm in thisAsList) {
            //    this.Insert(this.Count - 1, hltrvm);
            //}
            return result;
        }
        
        private async Task<Visibility> PerformHighlightAsync(string hlt) {
            var result = Visibility.Visible;
            await Dispatcher.CurrentDispatcher.InvokeAsync(
                (Action)(() => {
                    //var cb = ClipTileViewModel.ClipBorder;
                    var ttb = ClipTileViewModel.TitleTextBlock;

                    RegexOptions rot = RegexOptions.None;
                    RegexOptions roc = RegexOptions.None;
                    if (Properties.Settings.Default.SearchByIsCaseSensitive) {
                        rot = RegexOptions.ExplicitCapture;
                        roc = RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    } else {
                        rot = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
                        roc = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    }
                    var mct = Regex.Matches(ClipTileViewModel.CopyItemTitle, hlt, rot);
                    var mcc = Regex.Matches(ClipTileViewModel.CopyItemPlainText, hlt, roc);
                    var mcan = Regex.Matches(ClipTileViewModel.CopyItemAppName, hlt, roc);
                    var mcap = Regex.Matches(ClipTileViewModel.CopyItemAppPath, hlt, roc);

                    if (mct.Count == 0 && mcc.Count == 0 && mcan.Count == 0 && mcap.Count == 0) {                        
                        result = Visibility.Collapsed;
                        return;
                    }

                    Console.WriteLine("Beginning highlight clip with title: " + ClipTileViewModel.CopyItemTitle + " with highlight text: " + hlt);

                    //highlight title 
                    if(Properties.Settings.Default.SearchByTitle) {
                        if (ttb != null && mct.Count > 0) {
                            var trl = MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                            foreach (var mr in trl) {
                                this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, mr, (int)MpHighlightType.Title));
                            }
                        } else if (ttb == null) {
                            //
                        }
                    }
                    bool wasAppNameHighlighted = false;
                    if(Properties.Settings.Default.SearchByApplicationName) {
                        if(ClipTileViewModel.CopyItemAppName.ContainsByCaseSetting(hlt)) {
                            this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, (int)MpHighlightType.App));
                            wasAppNameHighlighted = true;
                        }
                    }
                    if (Properties.Settings.Default.SearchByProcessName && !wasAppNameHighlighted) {
                        if(!string.IsNullOrEmpty(ClipTileViewModel.CopyItemAppPath) &&
                           File.Exists(ClipTileViewModel.CopyItemAppPath)) {
                            string processName = Path.GetFileName(ClipTileViewModel.CopyItemAppPath);
                            if (ClipTileViewModel.CopyItemAppPath.ContainsByCaseSetting(hlt)) {
                                this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, null, (int)MpHighlightType.App));
                                wasAppNameHighlighted = true;
                            }
                        }                        
                    }
                    switch (ClipTileViewModel.CopyItemType) {
                        case MpCopyItemType.RichText:
                        case MpCopyItemType.Composite:
                            if(Properties.Settings.Default.SearchByRichText) {
                                for (int i = 0; i < ClipTileViewModel.RichTextBoxViewModelCollection.Count; i++) {
                                    var rtbvm = ClipTileViewModel.RichTextBoxViewModelCollection[i];
                                    var rtbvmtrl = MpHelpers.Instance.FindStringRangesFromPosition(rtbvm.Rtb.Document.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                    foreach (var mr in rtbvmtrl) {
                                        this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, mr, i));
                                    }
                                }
                            }
                            break;
                        case MpCopyItemType.Image:
                            // TODO Add filtering for images
                            if(Properties.Settings.Default.SearchByImage) {

                            }
                            break;
                        case MpCopyItemType.FileList:
                            if(Properties.Settings.Default.SearchByFileList) {
                                var flb = ClipTileViewModel.FileListBox;
                                for (int i = 0; i < ClipTileViewModel.FileListViewModels.Count; i++) {
                                    var fivm = ClipTileViewModel.FileListViewModels[i];
                                    if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                        var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                        if (container != null) {
                                            var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                            if (fitb != null) {
                                                var hlrl = MpHelpers.Instance.FindStringRangesFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                                foreach (var mr in hlrl) {
                                                    this.Add(new MpHighlightTextRangeViewModel(ClipTileViewModel, mr, i));
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

            return result;
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

        public void Dispose() {
            this.Clear();
        }
        #endregion

        #region Private Methods
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
            if (a.ContentId < b.ContentId) {
                return -1;
            }
            if (a.ContentId > b.ContentId) {
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
                return _selectNextMatchCommand;
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

        private RelayCommand _applyHighlightingCommand;
        public ICommand ApplyHighlightingCommand {
            get {
                if(_applyHighlightingCommand == null) {
                    _applyHighlightingCommand = new RelayCommand(ApplyHighlighting);
                }
                return _applyHighlightingCommand;
            }
        }
        private void ApplyHighlighting() {
            foreach(var hltrvm in this) {
                hltrvm.HighlightRange();
            }
        }

        private RelayCommand _hideHighlightingCommand;
        public ICommand HideHighlightingCommand {
            get {
                if (_hideHighlightingCommand == null) {
                    _hideHighlightingCommand = new RelayCommand(HideHighlighting);
                }
                return _hideHighlightingCommand;
            }
        }
        private void HideHighlighting() {
            foreach (var hltrvm in this) {
                hltrvm.ClearHighlighting();
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
