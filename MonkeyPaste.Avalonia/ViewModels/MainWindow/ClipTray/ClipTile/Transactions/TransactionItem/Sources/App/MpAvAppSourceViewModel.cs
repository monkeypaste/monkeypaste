using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppSourceViewModel : MpAvTransactionSourceViewModelBase {

        #region Interfaces
        #endregion

        #region Properties

        //public override object Body => base.Body;

        #region View Models

        public MpAvAppViewModel AppViewModel { get; private set; }

        #endregion

        #region Appearance

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvAppSourceViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            AppViewModel = 
                MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SourceObjId);

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(AppViewModel));
            OnPropertyChanged(nameof(Body));

            IsBusy = false;
        }

        #endregion

        #region Commands
        #endregion
    }
}
