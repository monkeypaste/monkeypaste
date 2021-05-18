using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MonkeyPaste
{
    public interface MpILocalStorage
    {
        Task Store(string filename);
        Task<List<string>> Get();
    }

}
