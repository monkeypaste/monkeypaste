using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;


namespace MonkeyPaste.Avalonia {
    public class MpContactTextTemplateViewModel : MpAvTextTemplateViewModelBase {
        #region Properties

        #region View Models

        #endregion

        public ObservableCollection<string> ContactFieldTypes {
            get {
                var cfts = new ObservableCollection<string>();
                for (int i = 0; i < Enum.GetValues(typeof(MpContactFieldType)).Length; i++) {
                    cfts.Add(((MpContactFieldType)i).ToString().ToLabel("Custom..."));
                }
                return cfts;
            }
        }

        #region State
        
        public int SelectedContactFieldTypeIdx {
            get {
                if(TextTemplate == null) {
                    return 0;
                }
                if(TemplateData == null) {
                    TemplateData = string.Empty;
                }
                var cft = ContactFieldTypes.FirstOrDefault(x => x.Replace(" ", string.Empty) == TemplateData);
                if(cft == null) {
                    return 0;
                }
                int cftIdx = ContactFieldTypes.IndexOf(cft);
                if(cftIdx < 0) {
                    return 0;
                }
                return cftIdx;
            }
            set {
                if(SelectedContactFieldTypeIdx != value) {
                    if(Enum.GetValues(typeof(MpContactFieldType)).Length <= value) {
                        TemplateData = string.Empty;
                    } else {
                        TemplateData = ((MpContactFieldType)value).ToString();
                    }
                    OnPropertyChanged(nameof(SelectedContactFieldTypeIdx));
                }
            }
        }


        public bool IsCustomFieldSelected => SelectedContactFieldTypeIdx == 0;

        #endregion

        #region Model

        private MpContactViewModel _selectedContact;
        public MpContactViewModel SelectedContact {
            get => _selectedContact;
            set {
                _selectedContact = value;

                TemplateText = _selectedContact.Contact.GetField(TemplateData) as string;
            }
        }
        #endregion

        #endregion

        #region Constructors
        public MpContactTextTemplateViewModel() : base(null) { }

        public MpContactTextTemplateViewModel(MpAvTemplateCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpContactTextTemplateViewModel_PropertyChanged;
        }

        #endregion
        
        #region Private Methods

        private void MpContactTextTemplateViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if(HostClipTileViewModel.IsPastingTemplate) {
                        if(!MpContactCollectionViewModel.Instance.IsLoaded) {
                            Dispatcher.UIThread.Post(async () => {
                                await MpContactCollectionViewModel.Instance.InitAsync();
                            });
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}
