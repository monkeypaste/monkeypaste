using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipTileRichTextViewModel : MpClipTileContentViewModel {
        private FlowDocument _documentRtf;
        public FlowDocument DocumentRtf {
            get {
                return _documentRtf;
            }
            set {
                if (_documentRtf != value) {
                    _documentRtf = value;
                    OnPropertyChanged(nameof(DocumentRtf));
                }
            }
        }

        private string _richText = string.Empty;
        public string RichText {
            get {
                return _richText;
            }
            set {
                if (_richText != value) {
                    _richText = value;
                    OnPropertyChanged(nameof(RichText));
                }
            }
        }
        #region Public Methods
        public MpClipTileRichTextViewModel(MpCopyItem copyItem, MpClipTileViewModel parent) : base(copyItem, parent) {
            RichText = (string)CopyItem.DataObject;      
            
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(RichText):
                        CopyItem.DataObject = RichText;
                        //updates flow document
                        using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(RichText))) {
                            TextRange range = new TextRange(DocumentRtf.ContentStart, DocumentRtf.ContentEnd);
                            range.Load(stream, DataFormats.Rtf);
                        }
                        //only write if the rt is changing after loading
                        if (!IsLoading && !((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).IsLoading) {
                            CopyItem.WriteToDatabase();
                        }                        
                        break;
                }
            };
        }
        #endregion

        #region Commands

        private RelayCommand _convertTokenToQrCodeCommand;
        public ICommand ConvertTokenToQrCodeCommand {
            get {
                if (_convertTokenToQrCodeCommand == null) {
                    _convertTokenToQrCodeCommand = new RelayCommand(ConvertTokenToQrCode);
                }
                return _convertTokenToQrCodeCommand;
            }
        }
        private bool CanConvertTokenToQrCode() {
            return CopyItem.CopyItemType != MpCopyItemType.Image && CopyItem.CopyItemType != MpCopyItemType.FileList;
        }
        private void ConvertTokenToQrCode() {
            
        }
        #endregion
    }
}
