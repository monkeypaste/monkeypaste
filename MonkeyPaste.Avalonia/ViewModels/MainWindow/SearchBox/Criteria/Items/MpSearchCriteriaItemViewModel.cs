using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gtk;
using MonkeyPaste;
using MonkeyPaste.Common;
namespace MonkeyPaste.Avalonia {


    #region Option Enums

    public enum MpRootOptionType {
        None = 0,
        Content,
        ContentType,
        Collection,
        Source,
        DateOrTime
    }

    public enum MpContentOptionType {
        None = 0,
        AnyText,
        TypeSpecific,
        Title
    }

    public enum MpContentTypeOptionType {
        None = 0,
        Text,
        Image,
        Files
    }

    public enum MpSourceOptionType {
        None = 0,
        Device,
        App,
        Website
    }

    public enum MpAppOptionType {
        None = 0,
        ApplicationName,
        ProcessPath
    }

    public enum MpWebsiteOptionType {
        None = 0,
        Url,
        Domain,
        Title
    }

    public enum MpDateTimeTypeOptionType {
        None = 0,
        Created,
        Modified,
        Pasted
    }

    public enum MpDateTimeOptionType {
        None = 0,
        WithinLast,
        Before,
        After,
        Exact
    }

    public enum MpFileContentOptionType {
        None = 0,
        Path,
        Name,
        Kind
    }

    public enum MpFileOptionType {
        None = 0,
        Document,
        Image,
        Video,
        Spreadsheet,
        Custom
    }

    public enum MpTextOptionType {
        None = 0,
        Matches,
        Contains,
        BeginsWith,
        EndsWith,
        RegEx
    }

    public enum MpImageOptionType {
        None = 0,
        Dimensions,
        Format,
        Description,
        Color
    }

    public enum MpNumberOptionType {
        None = 0,
        Equals,
        GreaterThan,
        LessThan,
        IsNot
    }

    public enum MpDimensionOptionType {
        None = 0,
        Width,
        Height
    }

    public enum MpColorOptionType {
        None = 0,
        Hex,
        RGBA
    }

    public enum MpTimeSpanWithinUnitType {
        None = 0,
        Hours,
        Days,
        Weeks,
        Months,
        Years
    }

    public enum MpDateBeforeUnitType {
        None = 0,
        Today,
        Yesterday,
        ThisWeek,
        ThisMonth,
        ThisYear,
        Exact
    }

    public enum MpDateAfterUnitType {
        None = 0,
        Yesterday,
        LastWeek,
        LastMonth,
        LastYear,
        Exact
    }

    #endregion

    public class MpAvSearchCriteriaItemViewModel : MpViewModelBase<MpAvSearchCriteriaItemCollectionViewModel> {
        #region Private Variables
        private List<string> _deviceNames;

        #endregion

        #region Constants

        public const string DEFAULT_OPTION_LABEL = " - Please Select - ";
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region ViewModels

        private ObservableCollection<MpAvSearchCriteriaOptionViewModel> _selectedOptions;
        public ObservableCollection<MpAvSearchCriteriaOptionViewModel> SelectedOptions {
            get {
                if(_selectedOptions == null) {
                    _selectedOptions = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();
                }
               // var tsovml = new ObservableCollection<MpAvSearchCriteriaOptionViewModel>();
                _selectedOptions.Clear();
                var node = RootOptionViewModel;
                while(node != null) {
                    _selectedOptions.Add(node);
                    int selIdx = node.Items.IndexOf(node.SelectedItem);
                    if(selIdx <= 0 || !node.HasChildren) {
                        break;
                    }
                    node = node.SelectedItem;
                }              
                
                return _selectedOptions;
            }
        }

        #region Options

        #region Default Labels

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
            var iovml = new List<MpAvSearchCriteriaOptionViewModel>();
            
            string[] labels = _deviceNames.ToArray();

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

        public MpAvSearchCriteriaOptionViewModel RootOptionViewModel { get; set; }

        #endregion

        #endregion

        #region Appearance
        #endregion

        #region State

        public bool IsCaseSensitive { get; set; } = false;

        public bool CanSetCaseSensitive { get; set; } = false;

        public bool IsInputVisible => 
            !SelectedOptions[SelectedOptions.Count - 1].HasChildren && 
            !SelectedOptions[SelectedOptions.Count - 1].UnitType.HasFlag(MpSearchCriteriaUnitFlags.EnumerableValue);

        public bool IsSelected { get; set; } = false;


        public int SortOrderIdx {
            get {
                if(SearchCriteriaItem == null) {
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

        #endregion

        #region Model

        

        public MpSearchCriteriaItem SearchCriteriaItem { get; set; }

        #endregion

        #endregion

        #region Public Methods

        public MpAvSearchCriteriaItemViewModel() : base(null) { }

        public MpAvSearchCriteriaItemViewModel(MpAvSearchCriteriaItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvSearchCriteriaItemViewModel_PropertyChanged;
        }

        public async Task InitializeAsync(MpSearchCriteriaItem sci) {
            IsBusy = true;

            SearchCriteriaItem = sci;

            var dl = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
            _deviceNames = dl.Select(x => x.MachineName).ToList();

            RootOptionViewModel = GetRootOption();
            OnPropertyChanged(nameof(SelectedOptions));

            IsBusy = false;
        }

        #endregion

        #region Private Methods
        

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
            }
        }


        #endregion

        #region Commands

        public ICommand AddNextCriteriaItemCommand => new MpCommand<object>(
            (args) => {
                bool isBooleanCriteria = args != null;

                Parent.AddSearchCriteriaItemCommand.Execute(this);
            },(args)=>Parent != null);
        
        public ICommand RemoveThisCriteriaItemCommand => new MpCommand(
            () => {
                Parent.RemoveSearchCriteriaItemCommand.Execute(this);
            },()=>Parent != null);




        #endregion
    }
}
