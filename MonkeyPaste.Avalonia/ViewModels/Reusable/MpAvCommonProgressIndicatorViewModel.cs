namespace MonkeyPaste.Avalonia {
    public class MpAvCommonProgressIndicatorViewModel : MpAvViewModelBase<MpAvViewModelBase>, MpIProgressIndicatorViewModel {
        public int TotalCount { get; set; }
        public int CurrentCount { get; set; }
        public double PercentLoaded =>
            TotalCount == 0 ? 1 : (double)CurrentCount / (double)TotalCount;
        public override bool IsLoaded =>
            PercentLoaded >= 1;

        public MpAvCommonProgressIndicatorViewModel(MpAvViewModelBase parent) : this(parent, 0, 0) { }
        public MpAvCommonProgressIndicatorViewModel(MpAvViewModelBase parent = default, int total = 0, int current = 0) : base(parent) {
            PropertyChanged += MpAvCommonProgressIndicatorViewModel_PropertyChanged;
            TotalCount = total;
            CurrentCount = current;
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
