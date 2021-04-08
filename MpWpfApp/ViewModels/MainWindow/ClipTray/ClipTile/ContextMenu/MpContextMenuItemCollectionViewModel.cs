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
        }

        public void Init() {
            this.Clear();
            if(Rtbvm == null) {
                return;
            }

            this.Add(new MpContextMenuItemViewModel(
                "Paste", 
                Rtbvm.RichTextBoxViewModelCollection.PasteSelectedClipsCommand,
                null, 
                false, 
                 Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/paste.png",
                 new ObservableCollection<MpContextMenuItemViewModel>{
                     new MpContextMenuItemViewModel("Test",null,null,false),
                     new MpContextMenuItemViewModel("Test",null,null,false) }));
            
            this.Add(null);

            this.Add(new MpContextMenuItemViewModel(
                "Bring To Front",
                Rtbvm.RichTextBoxViewModelCollection.BringSelectedClipTilesToFrontCommand,
                null,
                false,
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/bringToFront.png"));
            
            this.Add(null);
            
            this.Add(new MpContextMenuItemViewModel(
                "Send To Back",
                Rtbvm.RichTextBoxViewModelCollection.SendSelectedClipTilesToBackCommand,
                null,
                false,
                Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/sendToBack.png"));
        }
        #endregion

        #region Commands

        #endregion
    }
}
