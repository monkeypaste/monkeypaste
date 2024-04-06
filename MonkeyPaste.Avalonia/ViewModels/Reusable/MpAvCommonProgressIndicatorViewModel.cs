using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Threading;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCommonCancelableProgressIndicatorViewModel : MpAvCommonProgressIndicatorViewModel, MpICancelableProgressIndicatorViewModel {

        CancellationTokenSource _cts;
        CancellationToken MpICancelableProgressIndicatorViewModel.CancellationToken =>
            _cts == null ? default : _cts.Token;
        public bool WasCanceled =>
            _cts.IsCancellationRequested;
        public MpAvCommonCancelableProgressIndicatorViewModel(MpAvViewModelBase parent) : this(parent, 0, 0) { }
        public MpAvCommonCancelableProgressIndicatorViewModel(MpAvViewModelBase parent = default, int total = 0, int current = 0) : base(parent, total, current) {
            _cts = new CancellationTokenSource();
        }
        public ICommand CancelCommand => new MpCommand(() => {
            _cts.Cancel();
        });

        public override bool UpdateProgress(long totalBytes, long? bytesReceived, double percentComplete) {
            Dispatcher.UIThread.VerifyAccess();
            if (_cts.IsCancellationRequested) {
                return true;
            }
            return base.UpdateProgress(totalBytes, bytesReceived, percentComplete);
        }
    }
    public class MpAvCommonProgressIndicatorViewModel : MpAvViewModelBase<MpAvViewModelBase>, MpIProgressIndicatorViewModel {
        public long TotalCount { get; set; }
        public long CurrentCount { get; set; }
        public virtual double PercentLoaded =>
            TotalCount == 0 ? 0 : (double)CurrentCount / (double)TotalCount;
        public override bool IsLoaded =>
            PercentLoaded >= 1;

        public MpAvCommonProgressIndicatorViewModel(MpAvViewModelBase parent) : this(parent, 0, 0) { }
        public MpAvCommonProgressIndicatorViewModel(MpAvViewModelBase parent = default, int total = 0, int current = 0) : base(parent) {
            PropertyChanged += MpAvCommonProgressIndicatorViewModel_PropertyChanged;
            TotalCount = total;
            CurrentCount = current;
        }
        public virtual bool UpdateProgress(long totalBytes, long? bytesReceived, double percentComplete) {
            Dispatcher.UIThread.VerifyAccess();
            TotalCount = totalBytes;
            CurrentCount = bytesReceived.HasValue ? bytesReceived.Value : 0;
            OnPropertyChanged(nameof(PercentLoaded));
            return false;
        }


        private void MpAvCommonProgressIndicatorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(TotalCount):
                case nameof(CurrentCount):
                    OnPropertyChanged(nameof(PercentLoaded));
                    break;
            }
        }

        public override string ToString() {
            return $"Current: {CurrentCount} Total: {TotalCount} Percent: {PercentLoaded}";
        }
    }
}
