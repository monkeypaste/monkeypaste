using MonkeyPaste;
using System;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpDateTimeTextTemplateViewModel : MpAvTextTemplateViewModelBase {
        #region Properties

        
        public ObservableCollection<string> DefaultDateTimeFormats => new ObservableCollection<string>() {
            "Custom...", 
            "MM/dd/yyyy",
            "dddd, dd MMMM yyyy",
            "dddd, dd MMMM yyyy HH:mm",
            "dddd, dd MMMM yyyy HH:mm tt",
            "dddd, dd MMMM yyyy H:mm",
            "dddd, dd MMMM yyyy H:mm tt",
            "dddd, dd MMMM yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy hh:mm tt",
            "MM/dd/yyyy H:mm",
            "MM/dd/yyyy h:mm tt",
            "MM/dd/yyyy HH:mm:ss",
            "MMMM dd",
            "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK",
            "ddd, dd MMM yyy HH’:’mm’:’ss ‘GMT’",
            "yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss",
            "HH:mm",
            "hh:mm tt",
            "H:mm",
            "h:mm tt",
            "HH:mm:ss",
            "yyyy MMMM"
        };

        public ObservableCollection<string> DateTimeFormatItems {
            get {
                var dtfi = new ObservableCollection<string>();
                for (int i = 0; i < DefaultDateTimeFormats.Count; i++) {
                    if(i == 0) {
                        dtfi.Add(DefaultDateTimeFormats[i]);
                    } else {
                        dtfi.Add(DateTime.Now.ToString(DefaultDateTimeFormats[i]));
                    }                    
                }
                return dtfi;
            }
        }

        #region State

        public int SelectedDateTimeFormatIdx {
            get {
                if(TextTemplate == null) {
                    return -1;
                }
                int defaultIdx = DefaultDateTimeFormats.IndexOf(TemplateData);
                if(defaultIdx < 0) {
                    return 0;
                }
                return defaultIdx;
            }
            set {
                if (SelectedDateTimeFormatIdx != value) {
                    if (value == 0) {
                        TemplateData = string.Empty;
                    } else {
                        TemplateData = DefaultDateTimeFormats[value];
                    }
                    OnPropertyChanged(nameof(SelectedDateTimeFormatIdx));
                }
            }
        }

        public bool IsCustomFormatSelected => SelectedDateTimeFormatIdx == 0;

        #endregion

        #endregion

        #region Constructors
        public MpDateTimeTextTemplateViewModel() : base(null) { }

        public MpDateTimeTextTemplateViewModel(MpAvTemplateCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpDateTimeTextTemplateViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override void FillAutoTemplate() {
            TemplateText = DateTime.Now.ToString(TemplateData);
            OnPropertyChanged(nameof(TemplateDisplayValue));
        }

        #endregion

        #region Private Methods

        private void MpDateTimeTextTemplateViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(TemplateData):
                    OnPropertyChanged(nameof(SelectedDateTimeFormatIdx));
                    break;
            }
        }
        #endregion
    }
}
