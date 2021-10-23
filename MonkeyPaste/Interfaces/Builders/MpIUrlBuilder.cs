using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIUrlBuilder {
        Task<MpUrl> Create(string url);
    }
}
