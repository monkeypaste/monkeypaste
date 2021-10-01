using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpContentContextMenuViewModel : MpContextMenuViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        //public List<MpContextMenuItemViewModel> TagMenuItems {
        //    get {                
        //        var tmil = new List<MpContextMenuItemViewModel>();

        //        if (Parent == null) {
        //            return tmil;
        //        }
        //        foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
        //            if (tagTile.IsSudoTag) {
        //                continue;
        //            }
        //            tmil.Add(
        //                new MpContextMenuItemViewModel(
        //                    tagTile.TagName,
        //                    MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
        //                    tagTile,
        //                    tagTile.IsLinked(Parent.CopyItem),
        //                    string.Empty,
        //                    null,
        //                    tagTile.ShortcutKeyString,
        //                    tagTile.TagColor));
        //        }
        //        return tmil;
        //    }
        //}

        //private ObservableCollection<MpContextMenuItemViewModel> _translateLanguageMenuItems = null;
        //public ObservableCollection<MpContextMenuItemViewModel> TranslateLanguageMenuItems {
        //    get {
        //        if (_translateLanguageMenuItems == null) {
        //            _translateLanguageMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
        //            foreach (var languageName in MpLanguageTranslator.Instance.LanguageList) {
        //                _translateLanguageMenuItems.Add(new MpContextMenuItemViewModel(languageName, TranslateSelectedClipTextAsyncCommand, languageName, false));
        //            }
        //        }
        //        return _translateLanguageMenuItems;
        //    }
        //}

        #endregion


        #endregion

        #region Public Methods
        public MpContentContextMenuViewModel(MpContentItemViewModel parent) : base(parent) {

        }
        #endregion

        #region Commands

        #endregion
    }
}
