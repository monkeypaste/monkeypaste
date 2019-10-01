using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpHTMLTextItemTileController : MpCopyItemTileController {
        public MpHTMLTextItemTileController(int h,MpCopyItem ci) : base(h,ci) {
            string dataStr = (string)ci.DataObject;
            int idx0 = dataStr.IndexOf("<html>") < 0 ? 0:dataStr.IndexOf("<html>");
            int idx1 = dataStr.IndexOf("/<html>") < 0 ? dataStr.Length-1 : dataStr.IndexOf("/<html>");
            dataStr = dataStr.Substring(idx0,idx1 - idx0);
            dataStr.Insert(dataStr.IndexOf("<html>") + 4," style='border:none;'>");
            WebBrowser wb = new WebBrowser() {                     
                DocumentText = dataStr,
                Bounds = _contentPanel.Bounds,
                Location = new Point()
            };
            //((WebBrowser)_copyItemTile.itemControl).Font = new Font("Verdana",12.0f);
            _contentPanel.Controls.Add(_itemControl);
        }
    }
}
