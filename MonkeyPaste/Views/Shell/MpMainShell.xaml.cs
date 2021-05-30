using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Threading;

namespace MonkeyPaste
{
    public partial class MpMainShell : Shell
    {
        public MpMainShell()
        {
            InitializeComponent();
            //Routing.RegisterRoute("//tagitems", typeof(MpCopyItemCollectionView));
            //BindingContext = new MpMainShellViewModel();
        }
    }
}
