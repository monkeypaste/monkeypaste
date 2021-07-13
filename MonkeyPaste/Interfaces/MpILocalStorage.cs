using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MonkeyPaste
{
    public interface MpILocalStorage
    {
        bool CreateFile(string fileName, byte[] bytes, string fileType);        
    }

}
