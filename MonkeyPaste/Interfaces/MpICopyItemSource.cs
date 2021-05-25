using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpICopyItemSource {
        public abstract MpIcon SourceIcon { get; }
        public abstract string SourcePath { get; }
        public abstract string SourceName { get; }
        public abstract int RootId { get; }
        public abstract bool IsSubSource { get; }
    }
}
