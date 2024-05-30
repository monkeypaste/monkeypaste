using Avalonia;
using Avalonia.Animation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDirectionalPageSlide : PageSlide {
        public bool IsDirBackwards { get; set; }
        public MpAvDirectionalPageSlide(TimeSpan ts) : base(ts) { }
        public override async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken) {
            if(IsDirBackwards) {
                forward = !forward;
            }
            await base.Start(from, to, forward, cancellationToken);
        }
    }
}
