using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonoMac.Darwin;

namespace MonkeyPaste.Avalonia {

    public class MpAvSearchCriteriaItemViewModel : 
        MpViewModelBase<MpAvSearchCriteriaItemCollectionViewModel>,
        MpIQueryInfo {
        #region Private Variables
        

        #endregion

        #region Constants

        public const string DEFAULT_OPTION_LABEL = " - Please Select - ";
        #endregion

        #region Statics

        public static IEnumerable<Tuple<Enum, int>> GetContentFilterOptionPath(MpContentQueryBitFlags cft) {
            // 1: Option Enum
            // 2: Selected Option Idx

            switch (cft) {
                case MpContentQueryBitFlags.Content:

                    break;
            }
            return null;
        }

        #endregion

        #region Interfaces

        #region MpIQueryInfo Implementation

        public bool IsDescending =>
            MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
        public MpContentSortType SortType =>
            MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;

        public int TagId {
            get {
                int tag_id = MpTag.AllTagId;

                var tag_opts =
                    RootOptionViewModel
                    .SelfAndAllDescendants()
                    .Cast<MpAvSearchCriteriaOptionViewModel>()
                    .Where(x => x.FilterValue.HasFlag(MpContentQueryBitFlags.Tag))
                    .ToList();
                if(tag_opts.Count > 0) {
                    if(tag_opts.Count > 1) {
                        // how are there 2?
                        Debugger.Break();
                    }
                    try {
                        tag_id = int.Parse(tag_opts.FirstOrDefault().Value);
                    } catch(Exception ex) {
                        Debugger.Break();
                        MpConsole.WriteTraceLine("Error parsing tag id", ex);
                    }
                }
                return tag_id;
            }
        }
        public string SearchText {
            get {
                string st = string.Empty;
                if(LeafValueOptionViewModel != null) {
                    st = LeafValueOptionViewModel.Value;
                }
                return st;
            }
        }

        public MpContentQueryBitFlags FilterFlags { 
            get {
                return
                    RootOptionViewModel
                    .SelfAndAllDescendants()
                    .Cast<MpAvSearchCriteriaOptionViewModel>()
                    .Select(x=>x.FilterValue)
                    .Aggregate((a, b) => a | b);
            }        
        }

        // TODO need to update contentQuerier to use text flags or probably build off for advanced and use there
        public MpTextQueryType TextFlags => MpTextQueryType.None;
        public MpDateTimeQueryType TimeFlags { 
            get {
                MpDateTimeQueryType tfft = MpDateTimeQueryType.None;

                var time_opts =
                    RootOptionViewModel
                    .SelfAndAllDescendants()
                    .Cast<MpAvSearchCriteriaOptionViewModel>()
                    .Where(x => x.FilterValue.HasFlag(MpContentQueryBitFlags.DateTimeRange) ||
                                x.FilterValue.HasFlag(MpContentQueryBitFlags.DateTime))
                    .ToList();

                if (time_opts.Count > 0) {
                    if (time_opts.Count > 1) {
                        // how are there 2?
                        Debugger.Break();
                    }
                    var time_opt_vm = time_opts.FirstOrDefault();
                    MpDateTimeOptionType sel_opt = (MpDateTimeOptionType)
                        time_opt_vm.Parent.Items.IndexOf(time_opt_vm);
                    switch(sel_opt) {
                        case MpDateTimeOptionType.Before:
                            tfft = MpDateTimeQueryType.Before;
                            break;
                        case MpDateTimeOptionType.After:
                            tfft = MpDateTimeQueryType.After;
                            break;
                        case MpDateTimeOptionType.WithinLast:
                            tfft = MpDateTimeQueryType.Between;
                            break;
                        case MpDateTimeOptionType.Exact:
                            tfft = MpDateTimeQueryType.Between;
                            break;

                    }
                }
                return tfft;
            }
        }


        public MpIQueryInfo Next { 
            get {
                /*
                Join Notes:
                    Although it’s not obvious, you can also 
                    use Boolean search terms to set up a Finder 
                    search—to exclude criteria or to create an OR search. 
                    Once you have one condition set up, you can add a Boolean 
                    term to your next condition by option-clicking on 
                    the plus sign. The plus sign will turn into an ellipsis (…), 
                    and you’ll get a new pull-down menu with options for Any (OR), 
                    All (AND), or None (NOT). (For more details, see Add conditions 
                    to Finder searches) from https://www.macworld.com/article/189989/spotlight3.html
                */
                if (Parent == null) {
                    return null;
                }
                if(SortOrderIdx < Parent.Items.Count - 1) {
                    return Parent.SortedItems.ElementAt(SortOrderIdx + 1);
                }
                return null;
                
            }
        }     

        #endregion


        #endregion

        #region Properties

        #region ViewModels

        public MpAvSearchCriteriaOptionViewModel RootOptionViewModel { get; private set; }

        public MpAvSearchCriteriaOptionViewModel LeafValueOptionViewModel {
            get {
                var cur_ovm = RootOptionViewModel;
                while(cur_ovm.SelectedItem != null) {
                    cur_ovm = cur_ovm.SelectedItem;
                }

                var match_val_opts =
                    RootOptionViewModel
                    .SelfAndAllDescendants()
                    .Cast<MpAvSearchCriteriaOptionViewModel>()
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .ToList();
               
                return cur_ovm;
            }
        }

        private ObservableCollection<MpAvSearchCriteriaOptionViewModel> _items;
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> Items {
            get {
                if(_items == null) {
                    _items = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();
                }
                _items.Clear();
                var node = RootOptionViewModel;
                while(node != null) {
                    _items.Add(node);
                    int selIdx = node.Items.IndexOf(node.SelectedItem);
                    if(selIdx <= 0 || !node.HasChildren) {
                        break;
                    }
                    node = node.SelectedItem;
                }              
                
                return _items;
            }
        }

        private ObservableCollection<string> _joinTypeLabels;
        public ObservableCollection<string> JoinTypeLabels {
            get {
                if(_joinTypeLabels == null) {
                    _joinTypeLabels = new ObservableCollection<string>(
                        typeof(MpNextJoinOptionType)
                        .EnumToLabels()
                        .Skip(1));
                }
                return _joinTypeLabels;
            }
        }

        #region Options


        #region Root Option

        private MpAvSearchCriteriaOptionViewModel GetRootOption() {
            var rovm = new MpAvSearchCriteriaOptionViewModel(this, null);
            rovm.HostCriteriaItem = this;
            rovm.IsSelected = true;
            rovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
            rovm.Items.Clear();
            string[] labels = typeof(MpRootOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, rovm);
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.Label = labels[i];
                switch ((MpRootOptionType)i) {
                    case MpRootOptionType.Content:
                        tovm.Items = GetContentOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Collection:
                        tovm.Items = GetCollectionOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.Source:
                        tovm.Items = GetSourceOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.DateOrTime:
                        tovm.Items = GetDateTimeTypeOptionViewModel(tovm);
                        break;
                    case MpRootOptionType.ContentType:
                        tovm.Items = GetContentTypeOptionViewModel(tovm);
                        break;
                }
                rovm.Items.Add(tovm);
            }
            return rovm;
        }

        #endregion

        #region Content

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetTextOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var tovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpTextOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                if (i == labels.Length - 1) {
                    tovm.UnitType |= MpSearchCriteriaUnitFlags.RegEx;
                } else {
                    tovm.UnitType |= MpSearchCriteriaUnitFlags.CaseSensitivity;
                }
                tovml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(tovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetNumberOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpNumberOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Decimal;
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetColorOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpColorOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpColorOptionType)i) {
                    case MpColorOptionType.Hex:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Hex;
                        break;
                    case MpColorOptionType.RGBA:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.ByteX4 | MpSearchCriteriaUnitFlags.UnitDecimalX4;
                        break;
                }
                novml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDimensionsOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDimensionOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Integer;
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetImageContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpImageOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpImageOptionType)i) {
                    case MpImageOptionType.Dimensions:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetDimensionsOptionViewModel(ovm);
                        break;
                    case MpImageOptionType.Format:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        break;
                    case MpImageOptionType.Description:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpImageOptionType.Color:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetColorOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetFileOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpFileOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                if ((MpFileOptionType)i == MpFileOptionType.Custom) {
                    ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                    ovm.Items = GetTextOptionViewModel(ovm);
                } else {
                    ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetFileContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpFileContentOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpFileContentOptionType)i) {
                    case MpFileContentOptionType.Name:
                    case MpFileContentOptionType.Path:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpFileContentOptionType.Kind:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetFileOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentTypeContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpContentTypeOptionType)i) {
                    case MpContentTypeOptionType.Text:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentTypeOptionType.Image:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetImageContentOptionViewModel(ovm);
                        break;
                    case MpContentTypeOptionType.Files:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetFileContentOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpContentOptionType)i) {
                    case MpContentOptionType.AnyText:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.TypeSpecific:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetContentTypeContentOptionViewModel(ovm);
                        break;
                    case MpContentOptionType.Title:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Content Type Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetContentTypeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpContentTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                ovm.Label = labels[i];
                ovm.Value = ((MpCopyItemType)i).ToString();
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Collection Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetCollectionOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
            ovm.Label = "";
            ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
            ovm.Items = GetTextOptionViewModel(ovm);
            iovml.Add(ovm);
            foreach (var ttvm in MpAvTagTrayViewModel.Instance.Items.OrderBy(x => x.TagName)) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = ttvm.TagName;
                tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                iovml.Add(tovm);
            }
            var covm = new MpAvSearchCriteriaOptionViewModel(this, parent);
            covm.Label = " - Custom - ";
            covm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
            covm.Items = GetTextOptionViewModel(covm);
            iovml.Add(covm);
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Source Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDeviceOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            if (Parent == null) {
                return null;
            }
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();

            string[] labels = Parent.UserDevices.Select(x=>x.MachineName).ToArray();

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                ovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                iovml.Add(ovm);
            }
            var ovm1 = new MpAvSearchCriteriaOptionViewModel(this, parent);
            ovm1.Label = "";
            ovm1.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
            ovm1.Items = GetTextOptionViewModel(ovm1);
            iovml.Insert(0, ovm1);
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetAppOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpAppOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpAppOptionType)i) {
                    case MpAppOptionType.ProcessPath:
                    case MpAppOptionType.ApplicationName:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetWebsiteOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpWebsiteOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpWebsiteOptionType)i) {
                    case MpWebsiteOptionType.Domain:
                    case MpWebsiteOptionType.Url:
                    case MpWebsiteOptionType.Title:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Text;
                        ovm.Items = GetTextOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetSourceOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpSourceOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpSourceOptionType)i) {
                    case MpSourceOptionType.Device:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetDeviceOptionViewModel(ovm);
                        break;
                    case MpSourceOptionType.App:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetAppOptionViewModel(ovm);
                        break;
                    case MpSourceOptionType.Website:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetWebsiteOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion

        #region Date Options

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetTimeSpanWithinOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpTimeSpanWithinUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                tovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                tovm.Items = GetNumberOptionViewModel(tovm);
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateBeforeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateBeforeUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                switch ((MpDateBeforeUnitType)i) {
                    case MpDateBeforeUnitType.Exact:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        break;
                    default:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateAfterOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateAfterUnitType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var tovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                tovm.Label = labels[i];
                switch ((MpDateAfterUnitType)i) {
                    case MpDateAfterUnitType.Exact:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        break;
                    default:
                        tovm.UnitType = MpSearchCriteriaUnitFlags.EnumerableValue;
                        break;
                }
                novml.Add(tovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateTimeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var novml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateTimeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpDateTimeOptionType)i) {
                    case MpDateTimeOptionType.WithinLast:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetTimeSpanWithinOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.Before:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetDateBeforeOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.After:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetDateAfterOptionViewModel(ovm);
                        break;
                    case MpDateTimeOptionType.Exact:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.DateTime;
                        break;
                }
                novml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(novml);
        }

        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> GetDateTimeTypeOptionViewModel(MpAvSearchCriteriaOptionViewModel parent) {
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            string[] labels = typeof(MpDateTimeTypeOptionType).EnumToLabels(DEFAULT_OPTION_LABEL);

            for (int i = 0; i < labels.Length; i++) {
                var ovm = new MpAvSearchCriteriaOptionViewModel(this, parent);
                ovm.Label = labels[i];
                switch ((MpDateTimeTypeOptionType)i) {
                    case MpDateTimeTypeOptionType.Created:
                    case MpDateTimeTypeOptionType.Modified:
                    case MpDateTimeTypeOptionType.Pasted:
                        ovm.UnitType = MpSearchCriteriaUnitFlags.Enumerable;
                        ovm.Items = GetDateTimeOptionViewModel(ovm);
                        break;
                }
                iovml.Add(ovm);
            }
            return new ObservableCollection<MpAvSearchCriteriaOptionViewModel>(iovml);
        }

        #endregion


        #endregion

        #endregion

        #region Appearance

        public double CriteriaItemHeight =>
            Parent == null ? 0 :
                DefaultSearchCriteriaListBoxItemHeight * (IsJoinPanelVisible ? 2 : 1);
                
        #endregion

        #region Layout

        public double DefaultSearchCriteriaListBoxItemHeight => 50;

        public Thickness CriteriaItemBorder => new Thickness(0, 1, 0, 1);
        #endregion

        #region State

        public int SelectedJoinTypeIdx {
            get => (int)(NextJoinType - 1);
            set {
                if(value < 0) {
                    // BUG ui randomly setting idx to -1
                    return;
                }
                if(SelectedJoinTypeIdx != value) {
                    NextJoinType = (MpLogicalQueryType)(value + 1);
                    OnPropertyChanged(nameof(SelectedJoinTypeIdx));
                }
            }
        }

        public bool IsJoinDropDownOpen { get; set; }
        public bool CanRemoveThisCriteriaItem {
            get {
                if(Parent == null) {
                    return false;
                }
                if(Parent.Items.Count > 1) {
                    return true;
                }
                // reject last item remove
                return SortOrderIdx > 0;
            }
        }
        public bool IsAnyBusy => 
            IsBusy || Items.Any(x => x.IsAnyBusy);

        public bool IsJoinPanelVisible => 
            NextJoinType != MpSearchCriteriaItem.DEFAULT_QUERY_JOIN_TYPE;
        public bool IsCaseSensitive { get; set; } = false;

        public bool CanSetCaseSensitive { get; set; } = false;

        public bool IsInputVisible => 
            !Items[Items.Count - 1].HasChildren && 
            !Items[Items.Count - 1].UnitType.HasFlag(MpSearchCriteriaUnitFlags.EnumerableValue);

        public bool IsSelected { 
            get {
                if(Parent == null) {
                    return false;
                }
                return Parent.SelectedItem == this;
            }
        }

        #endregion

        #region Model

        public int SortOrderIdx {
            get {
                if (SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.SortOrderIdx;
            }
            set {
                if (SearchCriteriaItem.SortOrderIdx != value) {
                    SearchCriteriaItem.SortOrderIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public int SearchCriteriaItemId {
            get {
                if (SearchCriteriaItem == null) {
                    return 0;
                }
                return SearchCriteriaItem.Id;
            }
        }

        public string SearchOptions {
            get {
                if(SearchCriteriaItem == null) {
                    return null;
                }
                return SearchCriteriaItem.Options;
            }
        }


        public MpLogicalQueryType NextJoinType { 
            get {
                if(SearchCriteriaItem == null) {
                    return MpLogicalQueryType.None;
                }
                return SearchCriteriaItem.NextJoinType;
            }
            set {
                if(NextJoinType != value) {
                    SearchCriteriaItem.NextJoinType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    OnPropertyChanged(nameof(CriteriaItemHeight));
                    OnPropertyChanged(nameof(NextJoinType));
                }
            }        
        } 

        public MpSearchCriteriaItem SearchCriteriaItem { get; set; }


        #endregion

        #endregion

        #region Public Methods

        public MpAvSearchCriteriaItemViewModel() : this(null) { }

        public MpAvSearchCriteriaItemViewModel(MpAvSearchCriteriaItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchCriteriaItemViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        public async Task InitializeAsync(MpSearchCriteriaItem sci) {
            IsBusy = true;

            SearchCriteriaItem = sci;

            RootOptionViewModel = await CreateRootOptionViewModelAsync(SearchOptions);

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedJoinTypeIdx));

            IsBusy = false;
        }

        public void NotifyValueChanged() {
            Parent.NotifyQueryChanged(true);
        }

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {

        }
        private async Task<MpAvSearchCriteriaOptionViewModel> CreateRootOptionViewModelAsync(string options) {
            var root_opt_vm = GetRootOption();
            if(string.IsNullOrWhiteSpace(options)) {
                return root_opt_vm;
            }
            var cur_opt = root_opt_vm;
            var path_parts = options.SplitNoEmpty(",");
            for (int i = 0; i < path_parts.Length; i++) {
                if(cur_opt == null) {
                    // whats the option string? null should only be on last idx
                    Debugger.Break();
                    return GetRootOption();
                }
                string path_part = path_parts[i];
                try {
                    await cur_opt.InitializeAsync(path_part, i);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Criteria item id: {SearchCriteriaItemId} error. ex: ", ex);
                    return GetRootOption();
                }
                cur_opt = cur_opt.SelectedItem;
            }

            while(root_opt_vm.IsAnyBusy) {
                await Task.Delay(100);
            }
            return root_opt_vm;
        }

        

        private void MpAvSearchCriteriaItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            IsBusy = true;
                            await SearchCriteriaItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            IsBusy = false;
                        });
                    }
                    break;
                case nameof(IsJoinDropDownOpen):
                    OnPropertyChanged(nameof(IsJoinPanelVisible));
                    OnPropertyChanged(nameof(NextJoinType));
                    break;
                case nameof(IsJoinPanelVisible):
                    OnPropertyChanged(nameof(CriteriaItemHeight));
                    break;
                case nameof(CriteriaItemHeight):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.MaxSearchCriteriaListBoxHeight));
                    break;
            }
        }


        #endregion

        #region Commands

        public ICommand SelectCustomNextJoinTypeCommand => new MpCommand(
            () => {
                NextJoinType = MpLogicalQueryType.Or;
            },()=>NextJoinType == MpSearchCriteriaItem.DEFAULT_QUERY_JOIN_TYPE);

        public ICommand AddNextCriteriaItemCommand => new MpCommand(
            () => {
                Parent.AddSearchCriteriaItemCommand.Execute(this);
            },()=>Parent != null);
        
        public ICommand RemoveThisCriteriaItemCommand => new MpCommand(
            () => {
                Parent.RemoveSearchCriteriaItemCommand.Execute(this);
            },()=> CanRemoveThisCriteriaItem);
        #endregion
    }
}
