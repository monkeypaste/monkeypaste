using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIQuilEmbedable {
        /*having string ToHtml ToDocToken and  
         * GetTokenName(type w/ reflection) and
         * GetTokenId for rtf, templates, links 
         * (maybe iframe for web url?), auto-templates 
         * (like date, name, eventually contact element), 
         * image, audio, video, formula, csv (w|w/o header 
         * table can use rtf table logic?)*/
        string ToHtml();
        string ToDocToken();
        string GetTokenName();
        int GetTokenId();
    }
}
